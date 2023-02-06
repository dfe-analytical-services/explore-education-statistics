#nullable enable
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using GovUk.Education.ExploreEducationStatistics.Admin.Services.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Admin.Services.Interfaces.Cache;
using GovUk.Education.ExploreEducationStatistics.Admin.Services.Interfaces.Security;
using GovUk.Education.ExploreEducationStatistics.Admin.ViewModels;
using GovUk.Education.ExploreEducationStatistics.Common.Extensions;
using GovUk.Education.ExploreEducationStatistics.Common.Model;
using GovUk.Education.ExploreEducationStatistics.Common.Model.Chart;
using GovUk.Education.ExploreEducationStatistics.Common.Model.Data;
using GovUk.Education.ExploreEducationStatistics.Common.Services.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Common.Services.Interfaces.Security;
using GovUk.Education.ExploreEducationStatistics.Common.Utils;
using GovUk.Education.ExploreEducationStatistics.Content.Model;
using GovUk.Education.ExploreEducationStatistics.Content.Model.Database;
using GovUk.Education.ExploreEducationStatistics.Data.Model;
using GovUk.Education.ExploreEducationStatistics.Data.Model.Database;
using GovUk.Education.ExploreEducationStatistics.Data.Model.Repository.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Data.Services;
using GovUk.Education.ExploreEducationStatistics.Data.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using static GovUk.Education.ExploreEducationStatistics.Admin.Validators.ValidationErrorMessages;
using static GovUk.Education.ExploreEducationStatistics.Admin.Validators.ValidationUtils;
using IReleaseService = GovUk.Education.ExploreEducationStatistics.Admin.Services.Interfaces.IReleaseService;
using Release = GovUk.Education.ExploreEducationStatistics.Content.Model.Release;
using Unit = GovUk.Education.ExploreEducationStatistics.Common.Model.Unit;

namespace GovUk.Education.ExploreEducationStatistics.Admin.Services
{
    public class ReplacementService : IReplacementService
    {
        private readonly ContentDbContext _contentDbContext;
        private readonly StatisticsDbContext _statisticsDbContext;
        private readonly IFilterRepository _filterRepository;
        private readonly IIndicatorRepository _indicatorRepository;
        private readonly IIndicatorGroupRepository _indicatorGroupRepository;
        private readonly ILocationRepository _locationRepository;
        private readonly IFootnoteRepository _footnoteRepository;
        private readonly IReleaseService _releaseService;
        private readonly ITimePeriodService _timePeriodService;
        private readonly IPersistenceHelper<ContentDbContext> _contentPersistenceHelper;
        private readonly IUserService _userService;
        private readonly ICacheKeyService _cacheKeyService;
        private readonly IBlobCacheService _cacheService;

        private static IComparer<string> LabelComparer { get; } = new LabelRelationalComparer();

        public ReplacementService(ContentDbContext contentDbContext,
            StatisticsDbContext statisticsDbContext,
            IFilterRepository filterRepository,
            IIndicatorRepository indicatorRepository,
            IIndicatorGroupRepository indicatorGroupRepository,
            ILocationRepository locationRepository,
            IFootnoteRepository footnoteRepository,
            IReleaseService releaseService,
            ITimePeriodService timePeriodService,
            IPersistenceHelper<ContentDbContext> contentPersistenceHelper,
            IUserService userService,
            ICacheKeyService cacheKeyService,
            IBlobCacheService cacheService)
        {
            _contentDbContext = contentDbContext;
            _statisticsDbContext = statisticsDbContext;
            _filterRepository = filterRepository;
            _indicatorRepository = indicatorRepository;
            _indicatorGroupRepository = indicatorGroupRepository;
            _locationRepository = locationRepository;
            _footnoteRepository = footnoteRepository;
            _releaseService = releaseService;
            _timePeriodService = timePeriodService;
            _contentPersistenceHelper = contentPersistenceHelper;
            _userService = userService;
            _cacheKeyService = cacheKeyService;
            _cacheService = cacheService;
        }

        public async Task<Either<ActionResult, DataReplacementPlanViewModel>> GetReplacementPlan(
            Guid releaseId,
            Guid originalFileId,
            Guid replacementFileId)
        {
            return await _contentPersistenceHelper
                .CheckEntityExists<Release>(releaseId)
                .OnSuccess(_userService.CheckCanUpdateRelease)
                .OnSuccess(() => CheckFileExists(releaseId, originalFileId)
                    .OnSuccess(async originalFile =>
                    {
                        return await CheckFileExists(releaseId, replacementFileId)
                            .OnSuccess(async replacementFile =>
                            {
                                var originalSubjectId = originalFile.SubjectId!.Value;
                                var replacementSubjectId = replacementFile.SubjectId!.Value;

                                var replacementSubjectMeta = await GetReplacementSubjectMeta(replacementSubjectId);

                                var dataBlocks = ValidateDataBlocks(releaseId, originalSubjectId,
                                    replacementSubjectMeta);
                                var footnotes = await ValidateFootnotes(releaseId, originalSubjectId, replacementSubjectMeta);

                                return new DataReplacementPlanViewModel(
                                    dataBlocks,
                                    footnotes,
                                    originalSubjectId,
                                    replacementSubjectId);
                            });
                    }));
        }

        public async Task<Either<ActionResult, Unit>> Replace(
            Guid releaseId,
            Guid originalFileId,
            Guid replacementFileId)
        {
            return await GetReplacementPlan(releaseId, originalFileId, replacementFileId)
                .OnSuccess(async plan =>
                {
                    if (!plan.Valid)
                    {
                        return ValidationActionResult(ReplacementMustBeValid);
                    }

                    var originalSubjectId = plan.OriginalSubjectId;
                    var replacementSubjectId = plan.ReplacementSubjectId;

                    await plan.DataBlocks
                        .ToAsyncEnumerable()
                        .ForEachAwaitAsync(async dataBlockPlan =>
                        {
                            await InvalidateDataBlockCachedResults(dataBlockPlan, releaseId);
                            await ReplaceLinksForDataBlock(dataBlockPlan, replacementSubjectId);
                        });

                    await plan.Footnotes
                        .ToAsyncEnumerable()
                        .ForEachAwaitAsync(footnotePlan =>
                            ReplaceLinksForFootnote(footnotePlan, originalSubjectId, replacementSubjectId));

                    await ReplaceReleaseSubject(releaseId, originalSubjectId, replacementSubjectId);

                    await _contentDbContext.SaveChangesAsync();
                    await _statisticsDbContext.SaveChangesAsync();

                    return await RemoveOriginalSubjectAndFileFromRelease(releaseId, originalFileId, replacementFileId);
                });
        }

        private async Task<Either<ActionResult, File>> CheckFileExists(Guid releaseId, Guid fileId)
        {
            return await _contentPersistenceHelper.CheckEntityExists<ReleaseFile>(
                    q => q.Include(rf => rf.File)
                        .Where(rf => rf.ReleaseId == releaseId && rf.FileId == fileId)
                )
                .OnSuccess<ActionResult, ReleaseFile, File>(releaseFile =>
                    {
                        if (releaseFile.File.Type != FileType.Data)
                        {
                            return ValidationActionResult(ReplacementFileTypesMustBeData);
                        }

                        return releaseFile.File;
                    }
                );
        }

        private async Task<ReplacementSubjectMeta> GetReplacementSubjectMeta(Guid subjectId)
        {
            var filtersIncludingItems = await _filterRepository.GetFiltersIncludingItems(subjectId);

            var filters = filtersIncludingItems
                .ToDictionary(filter => filter.Name, filter => filter);

            var indicators = _indicatorRepository.GetIndicators(subjectId)
                .ToDictionary(indicator => indicator.Name, indicator => indicator);

            var locations = await _locationRepository.GetDistinctForSubject(subjectId);

            var timePeriods = await _timePeriodService.GetTimePeriods(subjectId);

            return new ReplacementSubjectMeta
            {
                Filters = filters,
                Indicators = indicators,
                Locations = locations,
                TimePeriods = timePeriods
            };
        }

        private List<DataBlockReplacementPlanViewModel> ValidateDataBlocks(Guid releaseId, Guid subjectId,
            ReplacementSubjectMeta replacementSubjectMeta)
        {
            return _contentDbContext
                .ReleaseContentBlocks
                .Include(join => join.ContentBlock)
                .Where(join => join.ReleaseId == releaseId)
                .Select(join => join.ContentBlock)
                .OfType<DataBlock>()
                .ToList()
                .Where(dataBlock => dataBlock.Query.SubjectId == subjectId)
                .Select(dataBlock => ValidateDataBlock(dataBlock, replacementSubjectMeta))
                .ToList();
        }

        private DataBlockReplacementPlanViewModel ValidateDataBlock(DataBlock dataBlock,
            ReplacementSubjectMeta replacementSubjectMeta)
        {
            var existingFilters = ValidateFiltersForDataBlock(dataBlock, replacementSubjectMeta);
            var newlyIntroducedFilters = FindNewlyIntroducedFiltersForDataBlock(dataBlock, replacementSubjectMeta);
            var indicatorGroups = ValidateIndicatorGroupsForDataBlock(dataBlock, replacementSubjectMeta);
            var locations = ValidateLocationsForDataBlock(dataBlock, replacementSubjectMeta);
            var timePeriods = ValidateTimePeriodsForDataBlock(dataBlock, replacementSubjectMeta);

            return new DataBlockReplacementPlanViewModel(dataBlock.Id,
                dataBlock.Name,
                existingFilters,
                newlyIntroducedFilters,
                indicatorGroups,
                locations,
                timePeriods);
        }

        private async Task<List<FootnoteReplacementPlanViewModel>> ValidateFootnotes(Guid releaseId, Guid subjectId,
            ReplacementSubjectMeta replacementSubjectMeta)
        {
            var footnotes = await _footnoteRepository.GetFootnotes(releaseId, subjectId);
            return footnotes
                .Select(footnote => ValidateFootnote(footnote, replacementSubjectMeta))
                .ToList();
        }

        private static FootnoteReplacementPlanViewModel ValidateFootnote(
            Footnote footnote,
            ReplacementSubjectMeta replacementSubjectMeta)
        {
            var filters = ValidateFiltersForFootnote(footnote, replacementSubjectMeta);
            var filterGroups = ValidateFilterGroupsForFootnote(footnote, replacementSubjectMeta);
            var filterItems = ValidateFilterItemsForFootnote(footnote, replacementSubjectMeta);
            var indicatorGroups = ValidateIndicatorGroupsForFootnote(footnote, replacementSubjectMeta);

            return new FootnoteReplacementPlanViewModel(
                footnote.Id,
                footnote.Content,
                filters,
                filterGroups,
                filterItems,
                indicatorGroups);
        }

        private static List<FootnoteFilterReplacementViewModel> ValidateFiltersForFootnote(
            Footnote footnote,
            ReplacementSubjectMeta replacementSubjectMeta)
        {
            return footnote.Filters
                .Select(filterFootnote => filterFootnote.Filter)
                .OrderBy(filter => filter.Label, LabelComparer)
                .Select(filter => new FootnoteFilterReplacementViewModel(
                    id: filter.Id,
                    label: filter.Label,
                    target: FindReplacementFilter(replacementSubjectMeta, filter.Name)?.Id
                ))
                .ToList();
        }

        private static List<FootnoteFilterGroupReplacementViewModel> ValidateFilterGroupsForFootnote(Footnote footnote,
            ReplacementSubjectMeta replacementSubjectMeta)
        {
            return footnote.FilterGroups
                .Select(filterGroupFootnote => filterGroupFootnote.FilterGroup)
                .OrderBy(filterGroup => filterGroup.Label, LabelComparer)
                .Select(filterGroup => new FootnoteFilterGroupReplacementViewModel(
                    id: filterGroup.Id,
                    label: filterGroup.Label,
                    filterId: filterGroup.FilterId,
                    filterLabel: filterGroup.Filter.Label,
                    target: FindReplacementFilterGroup(
                        replacementSubjectMeta,
                        filterGroup.Filter.Name,
                        filterGroup.Label
                    )?.Id
                ))
                .ToList();
        }

        private static List<FootnoteFilterItemReplacementViewModel> ValidateFilterItemsForFootnote(Footnote footnote,
            ReplacementSubjectMeta replacementSubjectMeta)
        {
            return footnote.FilterItems
                .Select(filterItemFootnote => filterItemFootnote.FilterItem)
                .OrderBy(filterItem => filterItem.Label, LabelComparer)
                .Select(filterItem => new FootnoteFilterItemReplacementViewModel(
                    id: filterItem.Id,
                    label: filterItem.Label,
                    filterId: filterItem.FilterGroup.FilterId,
                    filterLabel: filterItem.FilterGroup.Filter.Label,
                    filterGroupId: filterItem.FilterGroupId,
                    filterGroupLabel: filterItem.FilterGroup.Label,
                    target: FindReplacementFilterItem(
                        replacementSubjectMeta,
                        filterItem.FilterGroup.Filter.Name,
                        filterItem.FilterGroup.Label,
                        filterItem.Label
                    )?.Id
                ))
                .ToList();
        }

        private static Dictionary<Guid, IndicatorGroupReplacementViewModel> ValidateIndicatorGroupsForFootnote(
            Footnote footnote,
            ReplacementSubjectMeta replacementSubjectMeta)
        {
            return footnote.Indicators
                .Select(indicatorFootnote => indicatorFootnote.Indicator)
                .GroupBy(indicatorFootnote => indicatorFootnote.IndicatorGroup)
                .OrderBy(group => group.Key.Label, LabelComparer)
                .ToDictionary(
                    group => group.Key.Id,
                    group => new IndicatorGroupReplacementViewModel(
                        id: group.Key.Id,
                        label: group.Key.Label,
                        indicators: group.Select(indicator =>
                            ValidateIndicatorForReplacement(indicator, replacementSubjectMeta))
                    )
                );
        }

        private List<FilterReplacementViewModel> FindNewlyIntroducedFiltersForDataBlock(
            DataBlock dataBlock,
            ReplacementSubjectMeta replacementSubjectMeta)
        {
            var existingFilterItemIds = dataBlock
                .Query
                .Filters;

            var existingFilterNames = _statisticsDbContext
                .FilterItem
                .AsQueryable()
                .Where(fi => existingFilterItemIds.Contains(fi.Id))
                .Select(fi => fi.FilterGroup.Filter)
                .Select(f => f.Name)
                .Distinct()
                .ToList();

            return replacementSubjectMeta
                .Filters
                .Select(d => d.Value)
                .ToList()
                .Where(f => !existingFilterNames.Contains(f.Name))
                .Select(CreateNewlyIntroducedFilterReplacementViewModel)
                .ToList();
        }

        private static FilterReplacementViewModel CreateNewlyIntroducedFilterReplacementViewModel(Filter filter)
        {
            var filterGroupReplacementViewModels = filter
                .FilterGroups
                .Select(fg => new FilterGroupReplacementViewModel(fg.Id, fg.Label, fg
                    .FilterItems
                    .Select(fi => new FilterItemReplacementViewModel(fi.Id, fi.Label, null)))
                )
                .ToDictionary(f => f.Id);

            return new FilterReplacementViewModel(filter.Id, filter.Label, filter.Name,
                filterGroupReplacementViewModels);
        }

        private Dictionary<Guid, FilterReplacementViewModel> ValidateFiltersForDataBlock(
            DataBlock dataBlock,
            ReplacementSubjectMeta replacementSubjectMeta)
        {
            return _statisticsDbContext.FilterItem
                .AsQueryable()
                .Where(filterItem => dataBlock.Query.Filters.Contains(filterItem.Id))
                .Include(filterItem => filterItem.FilterGroup)
                .ThenInclude(filterGroup => filterGroup.Filter)
                .ToList()
                .GroupBy(filterItem => filterItem.FilterGroup.Filter)
                .OrderBy(filter => filter.Key.Label, LabelComparer)
                .ToDictionary(
                    filter => filter.Key.Id,
                    filter =>
                    {
                        return new FilterReplacementViewModel(
                            id: filter.Key.Id,
                            name: filter.Key.Name,
                            label: filter.Key.Label,
                            groups: filter
                                .GroupBy(filterItem => filterItem.FilterGroup)
                                .OrderBy(group => group.Key.Label, LabelComparer)
                                .ToDictionary(
                                    group => group.Key.Id,
                                    group => ValidateFilterGroupForReplacement(
                                        new FilterGroup
                                        {
                                            Id = group.Key.Id,
                                            Label = group.Key.Label,
                                            FilterItems = group.Key.FilterItems.Intersect(filter).ToList()
                                        },
                                        replacementSubjectMeta)
                                )
                        );
                    }
                );
        }

        private Dictionary<Guid, IndicatorGroupReplacementViewModel> ValidateIndicatorGroupsForDataBlock(
            DataBlock dataBlock,
            ReplacementSubjectMeta replacementSubjectMeta)
        {
            return _statisticsDbContext.Indicator
                .Include(indicator => indicator.IndicatorGroup)
                .Where(indicator => dataBlock.Query.Indicators.Contains(indicator.Id))
                .ToList()
                .GroupBy(indicator => indicator.IndicatorGroup)
                .OrderBy(group => group.Key.Label, LabelComparer)
                .ToDictionary(
                    group => group.Key.Id,
                    group => new IndicatorGroupReplacementViewModel(
                        id: group.Key.Id,
                        label: group.Key.Label,
                        indicators: group
                            .Select(indicator => ValidateIndicatorForReplacement(indicator, replacementSubjectMeta))
                            .OrderBy(indicator => indicator.Label, LabelComparer)
                    )
                );
        }

        private Dictionary<string, LocationReplacementViewModel> ValidateLocationsForDataBlock(
            DataBlock dataBlock,
            ReplacementSubjectMeta replacementSubjectMeta)
        {
            return _statisticsDbContext.Location
                .AsNoTracking()
                .Where(location => dataBlock.Query.LocationIds.Contains(location.Id))
                .ToList()
                .GroupBy(location => location.GeographicLevel)
                .ToDictionary(group => group.Key.ToString(),
                    group =>
                        new LocationReplacementViewModel(
                            label: group.Key.ToString(),
                            locationAttributes: group
                                .Select(location =>
                                    ValidateLocationForReplacement(
                                        location: location,
                                        level: group.Key,
                                        replacementSubjectMeta: replacementSubjectMeta))
                                .OrderBy(location => location.Label, LabelComparer)
                        )
                );
        }

        private static TimePeriodRangeReplacementViewModel ValidateTimePeriodsForDataBlock(
            DataBlock dataBlock,
            ReplacementSubjectMeta replacementSubjectMeta)
        {
            return new TimePeriodRangeReplacementViewModel(
                start: ValidateTimePeriodForReplacement(
                    dataBlock.Query.TimePeriod.StartYear,
                    dataBlock.Query.TimePeriod.StartCode,
                    replacementSubjectMeta
                ),
                end: ValidateTimePeriodForReplacement(
                    dataBlock.Query.TimePeriod.EndYear,
                    dataBlock.Query.TimePeriod.EndCode,
                    replacementSubjectMeta
                )
            );
        }

        private static TimePeriodReplacementViewModel ValidateTimePeriodForReplacement(
            int year,
            TimeIdentifier code,
            ReplacementSubjectMeta replacementSubjectMeta)
        {
            return new TimePeriodReplacementViewModel(
                year: year,
                code: code,
                valid: replacementSubjectMeta.TimePeriods.Contains((year, code))
            );
        }

        private static FilterGroupReplacementViewModel ValidateFilterGroupForReplacement(
            FilterGroup filterGroup,
            ReplacementSubjectMeta replacementSubjectMeta)
        {
            return new FilterGroupReplacementViewModel(
                id: filterGroup.Id,
                label: filterGroup.Label,
                filters: filterGroup.FilterItems
                    .Select(item => ValidateFilterItemForReplacement(item, replacementSubjectMeta))
                    .OrderBy(item => item.Label, LabelComparer)
            );
        }

        private static FilterItemReplacementViewModel ValidateFilterItemForReplacement(
            FilterItem filterItem,
            ReplacementSubjectMeta replacementSubjectMeta)
        {
            return new FilterItemReplacementViewModel(
                id: filterItem.Id,
                label: filterItem.Label,
                target: FindReplacementFilterItem(
                    replacementSubjectMeta,
                    filterItem.FilterGroup.Filter.Name,
                    filterItem.FilterGroup.Label,
                    filterItem.Label)?.Id
            );
        }

        private static IndicatorReplacementViewModel ValidateIndicatorForReplacement(
            Indicator indicator,
            ReplacementSubjectMeta replacementSubjectMeta)
        {
            return new IndicatorReplacementViewModel(
                id: indicator.Id,
                name: indicator.Name,
                label: indicator.Label,
                target: FindReplacementIndicator(replacementSubjectMeta, indicator.Name)
            );
        }

        private static LocationAttributeReplacementViewModel ValidateLocationForReplacement(
            Location location,
            GeographicLevel level,
            ReplacementSubjectMeta replacementSubjectMeta)
        {
            var locationAttribute = location.ToLocationAttribute();
            var code = locationAttribute.GetCodeOrFallback();

            // If the replacement subject contains the same location by id then use it,
            // otherwise try to find a location with the same code
            var target = replacementSubjectMeta.Locations
                             .SingleOrDefault(l => l.Id == location.Id)?.Id ??
                         FindReplacementLocation(replacementSubjectMeta, level, code);

            return new LocationAttributeReplacementViewModel(
                id: location.Id,
                code: code,
                label: locationAttribute.Name ?? string.Empty,
                target: target
            );
        }

        private static Filter? FindReplacementFilter(ReplacementSubjectMeta replacementSubjectMeta,
            string filterName)
        {
            return replacementSubjectMeta.Filters.GetValueOrDefault(filterName);
        }

        private static FilterGroup? FindReplacementFilterGroup(ReplacementSubjectMeta replacementSubjectMeta,
            string filterName,
            string filterGroupLabel)
        {
            var replacementFilter = FindReplacementFilter(replacementSubjectMeta, filterName);
            return replacementFilter?.FilterGroups.SingleOrDefault(filterGroup =>
                filterGroup.Label == filterGroupLabel);
        }

        private static FilterItem? FindReplacementFilterItem(ReplacementSubjectMeta replacementSubjectMeta,
            string filterName,
            string filterGroupLabel,
            string filterItemLabel)
        {
            var replacementFilterGroup =
                FindReplacementFilterGroup(replacementSubjectMeta, filterName, filterGroupLabel);
            return replacementFilterGroup?.FilterItems.SingleOrDefault(filterItem =>
                filterItem.Label == filterItemLabel);
        }

        private static Guid? FindReplacementIndicator(ReplacementSubjectMeta replacementSubjectMeta,
            string indicatorName)
        {
            return replacementSubjectMeta.Indicators.GetValueOrDefault(indicatorName)?.Id;
        }

        private static Guid? FindReplacementLocation(ReplacementSubjectMeta replacementSubjectMeta,
            GeographicLevel level,
            string locationCode)
        {
            var candidateReplacements = replacementSubjectMeta.Locations
                .Where(location =>
                    // Filter by level as the other locations are unlikely to have matching codes
                    // and even if they did we shouldn't target a replacement location with a different level
                    location.GeographicLevel == level &&
                    location.ToLocationAttribute().GetCodeOrFallback() == locationCode)
                .ToList();

            // Only return a location if there's exactly one.
            // We could try and reduce the chance of there being multiple by matching on name as well as code,
            // but this would restrict replacements from correcting location names.
            return candidateReplacements.Count == 1 ? candidateReplacements[0].Id : null;
        }

        private async Task ReplaceLinksForDataBlock(DataBlockReplacementPlanViewModel replacementPlan,
            Guid replacementSubjectId)
        {
            var dataBlock = await _contentDbContext.ContentBlocks
                .AsQueryable()
                .OfType<DataBlock>()
                .SingleAsync(block => block.Id == replacementPlan.Id);

            _contentDbContext.Update(dataBlock);

            dataBlock.Query.SubjectId = replacementSubjectId;
            ReplaceDataBlockQueryFilters(replacementPlan, dataBlock);
            ReplaceDataBlockQueryIndicators(replacementPlan, dataBlock);
            ReplaceDataBlockQueryLocations(replacementPlan, dataBlock);

            var filterItemTargets = replacementPlan.Filters
                .SelectMany(filter =>
                    filter.Value.Groups.SelectMany(group => group.Value.Filters))
                .ToDictionary(ReplacementPlanOriginalId, ReplacementPlanTargetId);
            var indicatorTargets = replacementPlan.IndicatorGroups
                .SelectMany(group => group.Value.Indicators)
                .ToDictionary(ReplacementPlanOriginalId, ReplacementPlanTargetId);
            var locationTargets = replacementPlan.Locations
                .Values
                .SelectMany(group => group.LocationAttributes)
                .ToDictionary(ReplacementPlanOriginalId, ReplacementPlanTargetId);

            ReplaceDataBlockTableHeaders(
                filterItemTargets: filterItemTargets,
                indicatorTargets: indicatorTargets,
                locationTargets: locationTargets,
                dataBlock);
            ReplaceDataBlockCharts(
                filterItemTargets: filterItemTargets,
                indicatorTargets: indicatorTargets,
                locationTargets: locationTargets,
                dataBlock);
        }

        private static void ReplaceDataBlockQueryFilters(DataBlockReplacementPlanViewModel replacementPlan,
            DataBlock dataBlock)
        {
            var filterItems = dataBlock.Query.Filters.ToList();

            replacementPlan.Filters
                .SelectMany(filter =>
                    filter.Value.Groups.SelectMany(group => group.Value.Filters))
                .ToList()
                .ForEach(plan =>
                {
                    filterItems.Remove(plan.Id);
                    filterItems.Add(plan.TargetValue);
                });

            dataBlock.Query.Filters = filterItems;
        }

        private static void ReplaceDataBlockQueryIndicators(DataBlockReplacementPlanViewModel replacementPlan,
            DataBlock dataBlock)
        {
            var indicators = dataBlock.Query.Indicators.ToList();

            replacementPlan.IndicatorGroups
                .SelectMany(group => group.Value.Indicators)
                .ToList()
                .ForEach(plan =>
                {
                    indicators.Remove(plan.Id);
                    indicators.Add(plan.TargetValue);
                });

            dataBlock.Query.Indicators = indicators;
        }

        private static void ReplaceDataBlockQueryLocations(DataBlockReplacementPlanViewModel replacementPlan,
            DataBlock dataBlock)
        {
            var locations = dataBlock.Query.LocationIds.ToList();

            replacementPlan.Locations
                .Values
                .SelectMany(group => group.LocationAttributes)
                .ToList()
                .ForEach(plan =>
                {
                    locations.Remove(plan.Id);
                    locations.Add(plan.TargetValue);
                });

            dataBlock.Query.LocationIds = locations;
        }

        private static void ReplaceDataBlockTableHeaders(
            IReadOnlyDictionary<Guid, Guid> filterItemTargets,
            IReadOnlyDictionary<Guid, Guid> indicatorTargets,
            IReadOnlyDictionary<Guid, Guid> locationTargets,
            DataBlock dataBlock)
        {
            var tableHeaders = dataBlock.Table.TableHeaders;

            // Replace Columns
            ReplaceDataBlockTableHeaders(
                tableHeaders.Columns.FilterByType(TableHeaderType.Filter), dataBlock, filterItemTargets);
            ReplaceDataBlockTableHeaders(
                tableHeaders.Columns.FilterByType(TableHeaderType.Indicator), dataBlock, indicatorTargets);
            ReplaceDataBlockTableHeaders(
                tableHeaders.Columns.FilterByType(TableHeaderType.Location), dataBlock, locationTargets);

            // Replace Rows
            ReplaceDataBlockTableHeaders(
                tableHeaders.Rows.FilterByType(TableHeaderType.Filter), dataBlock, filterItemTargets);
            ReplaceDataBlockTableHeaders(
                tableHeaders.Rows.FilterByType(TableHeaderType.Indicator), dataBlock, indicatorTargets);
            ReplaceDataBlockTableHeaders(
                tableHeaders.Rows.FilterByType(TableHeaderType.Location), dataBlock, locationTargets);

            // Replace Column Groups
            tableHeaders.ColumnGroups.ForEach(group =>
            {
                ReplaceDataBlockTableHeaders(
                    group.FilterByType(TableHeaderType.Filter), dataBlock, filterItemTargets);

                ReplaceDataBlockTableHeaders(
                    group.FilterByType(TableHeaderType.Indicator), dataBlock, indicatorTargets);

                ReplaceDataBlockTableHeaders(
                    group.FilterByType(TableHeaderType.Location), dataBlock, locationTargets);
            });

            // Replace Row Groups
            tableHeaders.RowGroups.ForEach(group =>
            {
                ReplaceDataBlockTableHeaders(
                    group.FilterByType(TableHeaderType.Filter), dataBlock, filterItemTargets);

                ReplaceDataBlockTableHeaders(
                    group.FilterByType(TableHeaderType.Indicator), dataBlock, indicatorTargets);

                ReplaceDataBlockTableHeaders(
                    group.FilterByType(TableHeaderType.Location), dataBlock, locationTargets);
            });
        }

        private static void ReplaceDataBlockTableHeaders(
            List<TableHeader> tableHeaders,
            DataBlock dataBlock,
            IReadOnlyDictionary<Guid, Guid> targets)
        {
            foreach (var tableHeader in tableHeaders)
            {
                if (Guid.TryParse(tableHeader.Value, out var idAsGuid))
                {
                    if (targets.TryGetValue(idAsGuid, out var targetId))
                    {
                        tableHeader.Value = targetId.ToString();
                    }
                    else
                    {
                        throw new InvalidOperationException(
                            $"Expected target replacement value for dataBlock {dataBlock.Id} {tableHeader.Type} table header value: {idAsGuid}");
                    }
                }
                else
                {
                    throw new InvalidOperationException(
                        $"Expected Guid for dataBlock {dataBlock.Id} {tableHeader.Type} table header value but found: {tableHeader.Value}");
                }
            }
        }

        private static void ReplaceDataBlockCharts(
            IReadOnlyDictionary<Guid, Guid> filterItemTargets,
            IReadOnlyDictionary<Guid, Guid> indicatorTargets,
            IReadOnlyDictionary<Guid, Guid> locationTargets,
            DataBlock dataBlock)
        {
            dataBlock.Charts.ForEach(
                chart =>
                {
                    ReplaceChartMajorAxisDataSets(
                        filterItemTargets: filterItemTargets,
                        indicatorTargets: indicatorTargets,
                        locationTargets: locationTargets,
                        dataBlock,
                        chart);
                    ReplaceChartLegendDataSets(
                        filterItemTargets: filterItemTargets,
                        indicatorTargets: indicatorTargets,
                        locationTargets: locationTargets,
                        dataBlock,
                        chart);
                }
            );
        }

        private static void ReplaceChartLegendDataSets(
            IReadOnlyDictionary<Guid, Guid> filterItemTargets,
            IReadOnlyDictionary<Guid, Guid> indicatorTargets,
            IReadOnlyDictionary<Guid, Guid> locationTargets,
            DataBlock dataBlock,
            IChart chart)
        {
            chart.Legend?.Items.ForEach(
                item =>
                {
                    var dataSet = item.DataSet;

                    dataSet.Filters = dataSet.Filters.Select(
                        filter =>
                        {
                            if (filterItemTargets.TryGetValue(filter, out var targetFilterId))
                            {
                                return targetFilterId;
                            }

                            throw new InvalidOperationException(
                                $"Expected target replacement value for dataBlock {dataBlock.Id} chart legend data set filter: {filter}"
                            );
                        }
                    ).ToList();


                    if (dataSet.Indicator.HasValue)
                    {
                        if (indicatorTargets.TryGetValue(dataSet.Indicator.Value, out var targetIndicatorId))
                        {
                            dataSet.Indicator = targetIndicatorId;
                        }
                        else
                        {
                            throw new InvalidOperationException(
                                $"Expected target replacement value for dataBlock {dataBlock.Id} chart legend data set indicator: {dataSet.Indicator}"
                            );
                        }
                    }

                    if (dataSet.Location != null)
                    {
                        if (locationTargets.TryGetValue(dataSet.Location.Value, out var targetLocationId))
                        {
                            dataSet.Location.Value = targetLocationId;
                        }
                        else
                        {
                            throw new InvalidOperationException(
                                $"Expected target replacement value for dataBlock {dataBlock.Id} chart legend data set location: {dataSet.Location.Value}"
                            );
                        }
                    }
                }
            );
        }

        private static void ReplaceChartMajorAxisDataSets(
            IReadOnlyDictionary<Guid, Guid> filterItemTargets,
            IReadOnlyDictionary<Guid, Guid> indicatorTargets,
            IReadOnlyDictionary<Guid, Guid> locationTargets,
            DataBlock dataBlock,
            IChart chart)
        {
            chart.Axes?.GetValueOrDefault("major")?.DataSets.ForEach(
                dataSet =>
                {
                    dataSet.Filters = dataSet.Filters.Select(
                        filter =>
                        {
                            if (filterItemTargets.TryGetValue(filter, out var targetFilterId))
                            {
                                return targetFilterId;
                            }

                            throw new InvalidOperationException(
                                $"Expected target replacement value for dataBlock {dataBlock.Id} chart data set filter: {filter}"
                            );
                        }
                    ).ToList();

                    if (indicatorTargets.TryGetValue(dataSet.Indicator, out var targetIndicatorId))
                    {
                        dataSet.Indicator = targetIndicatorId;
                    }
                    else
                    {
                        throw new InvalidOperationException(
                            $"Expected target replacement value for dataBlock {dataBlock.Id} chart data set indicator: {dataSet.Indicator}"
                        );
                    }

                    if (dataSet.Location != null)
                    {
                        if (locationTargets.TryGetValue(dataSet.Location.Value, out var targetLocationId))
                        {
                            dataSet.Location.Value = targetLocationId;
                        }
                        else
                        {
                            throw new InvalidOperationException(
                                $"Expected target replacement value for dataBlock {dataBlock.Id} chart data set location: {dataSet.Location.Value}"
                            );
                        }
                    }
                }
            );
        }

        private async Task ReplaceLinksForFootnote(FootnoteReplacementPlanViewModel replacementPlan,
            Guid originalSubjectId,
            Guid replacementSubjectId)
        {
            await ReplaceFootnoteSubject(replacementPlan.Id, originalSubjectId, replacementSubjectId);

            await replacementPlan.Filters
                .ToAsyncEnumerable()
                .ForEachAwaitAsync(async plan =>
                    await ReplaceFootnoteFilter(replacementPlan.Id, plan));

            await replacementPlan.FilterGroups
                .ToAsyncEnumerable()
                .ForEachAwaitAsync(async plan =>
                    await ReplaceFootnoteFilterGroup(replacementPlan.Id, plan));

            await replacementPlan.FilterItems
                .ToAsyncEnumerable()
                .ForEachAwaitAsync(async plan =>
                    await ReplaceFootnoteFilterItem(replacementPlan.Id, plan));

            await replacementPlan.IndicatorGroups
                .SelectMany(group => group.Value.Indicators)
                .ToAsyncEnumerable()
                .ForEachAwaitAsync(async plan =>
                    await ReplaceIndicatorFootnote(replacementPlan.Id, plan));
        }

        private async Task ReplaceFootnoteSubject(Guid footnoteId, Guid originalSubjectId, Guid replacementSubjectId)
        {
            var subjectFootnote = await _statisticsDbContext.SubjectFootnote
                .AsQueryable()
                .Where(f =>
                    f.FootnoteId == footnoteId && f.SubjectId == originalSubjectId).SingleOrDefaultAsync();

            if (subjectFootnote != null)
            {
                _statisticsDbContext.Remove(subjectFootnote);
                await _statisticsDbContext.SubjectFootnote.AddAsync(new SubjectFootnote
                {
                    FootnoteId = footnoteId,
                    SubjectId = replacementSubjectId
                });
            }
        }

        private async Task ReplaceFootnoteFilter(Guid footnoteId, TargetableReplacementViewModel plan)
        {
            var filterFootnote = await _statisticsDbContext.FilterFootnote
                .AsQueryable()
                .SingleAsync(f =>
                    f.FootnoteId == footnoteId && f.FilterId == plan.Id
                );

            _statisticsDbContext.Remove(filterFootnote);
            await _statisticsDbContext.FilterFootnote.AddAsync(new FilterFootnote
            {
                FootnoteId = footnoteId,
                FilterId = plan.TargetValue
            });
        }

        private async Task ReplaceFootnoteFilterGroup(Guid footnoteId, TargetableReplacementViewModel plan)
        {
            var filterGroupFootnote = await _statisticsDbContext.FilterGroupFootnote
                .AsQueryable()
                .SingleAsync(f =>
                    f.FootnoteId == footnoteId && f.FilterGroupId == plan.Id
                );

            _statisticsDbContext.Remove(filterGroupFootnote);
            await _statisticsDbContext.FilterGroupFootnote.AddAsync(new FilterGroupFootnote
            {
                FootnoteId = footnoteId,
                FilterGroupId = plan.TargetValue
            });
        }

        private async Task ReplaceFootnoteFilterItem(Guid footnoteId, TargetableReplacementViewModel plan)
        {
            var filterItemFootnote = await _statisticsDbContext.FilterItemFootnote
                .AsQueryable()
                .SingleAsync(f =>
                    f.FootnoteId == footnoteId && f.FilterItemId == plan.Id
                );

            _statisticsDbContext.Remove(filterItemFootnote);
            await _statisticsDbContext.FilterItemFootnote.AddAsync(new FilterItemFootnote
            {
                FootnoteId = footnoteId,
                FilterItemId = plan.TargetValue
            });
        }

        private async Task ReplaceIndicatorFootnote(Guid footnoteId, TargetableReplacementViewModel plan)
        {
            var indicatorFootnote = await _statisticsDbContext.IndicatorFootnote
                .AsQueryable()
                .SingleAsync(f =>
                    f.FootnoteId == footnoteId && f.IndicatorId == plan.Id
                );

            _statisticsDbContext.Remove(indicatorFootnote);
            await _statisticsDbContext.IndicatorFootnote.AddAsync(new IndicatorFootnote
            {
                FootnoteId = footnoteId,
                IndicatorId = plan.TargetValue
            });
        }

        private async Task ReplaceReleaseSubject(Guid releaseId,
            Guid originalSubjectId,
            Guid replacementSubjectId)
        {
            var originalReleaseSubject = await _statisticsDbContext.ReleaseSubject
                .SingleAsync(rs => rs.ReleaseId == releaseId &&
                                   rs.SubjectId == originalSubjectId);

            var replacementReleaseSubject = await _statisticsDbContext.ReleaseSubject
                .SingleAsync(rs => rs.ReleaseId == releaseId &&
                                   rs.SubjectId == replacementSubjectId);

            _statisticsDbContext.Update(replacementReleaseSubject);

            replacementReleaseSubject.DataGuidance = originalReleaseSubject.DataGuidance;

            replacementReleaseSubject.FilterSequence =
                await ReplaceFilterSequence(originalReleaseSubject, replacementReleaseSubject);
            replacementReleaseSubject.IndicatorSequence =
                await ReplaceIndicatorSequence(originalReleaseSubject, replacementReleaseSubject);
        }

        private async Task<List<FilterSequenceEntry>?> ReplaceFilterSequence(ReleaseSubject originalReleaseSubject,
            ReleaseSubject replacementReleaseSubject)
        {
            // If the sequence is undefined then leave it so we continue to fallback to ordering by label alphabetically
            if (originalReleaseSubject.FilterSequence == null)
            {
                return null;
            }

            var originalFilters = 
                await _filterRepository.GetFiltersIncludingItems(originalReleaseSubject.SubjectId);
            var replacementFilters =
                await _filterRepository.GetFiltersIncludingItems(replacementReleaseSubject.SubjectId);

            return ReplacementServiceHelper.ReplaceFilterSequence(
                originalFilters: originalFilters,
                replacementFilters: replacementFilters,
                originalReleaseSubject);
        }

        private async Task<List<IndicatorGroupSequenceEntry>?> ReplaceIndicatorSequence(
            ReleaseSubject originalReleaseSubject,
            ReleaseSubject replacementReleaseSubject)
        {
            // If the sequence is undefined then leave it so we continue to fallback to ordering by label alphabetically
            if (originalReleaseSubject.IndicatorSequence == null)
            {
                return null;
            }

            var originalIndicatorGroups =
                await _indicatorGroupRepository.GetIndicatorGroups(originalReleaseSubject.SubjectId);
            var replacementIndicatorGroups =
                await _indicatorGroupRepository.GetIndicatorGroups(replacementReleaseSubject.SubjectId);

            return ReplacementServiceHelper.ReplaceIndicatorSequence(
                originalIndicatorGroups: originalIndicatorGroups,
                replacementIndicatorGroups: replacementIndicatorGroups,
                originalReleaseSubject);
        }

        private async Task<Either<ActionResult, Unit>> RemoveOriginalSubjectAndFileFromRelease(
            Guid releaseId,
            Guid originalFileId,
            Guid replacementFileId)
        {
            // First, unlink the original file from the replacement before removing it.
            // Ordinarily, removing a file from a Release deletes any associated replacement
            // so that there's no possibility of abandoned replacements being orphaned from their original files.
            return await CheckFileExists(releaseId, originalFileId)
                .OnSuccessVoid(async originalFile =>
                {
                    await CheckFileExists(releaseId, replacementFileId)
                        .OnSuccessVoid(
                            async replacementFile =>
                            {
                                if (originalFile.ReplacedById != replacementFile.Id)
                                {
                                    throw new InvalidOperationException(
                                        $"Expected the original file reference to be associated with the replacement but found: {originalFile.ReplacedById}");
                                }

                                originalFile.ReplacedById = null;
                                replacementFile.ReplacingId = null;
                                _contentDbContext.Update(originalFile);
                                _contentDbContext.Update(replacementFile);

                                await _contentDbContext.SaveChangesAsync();
                            });
                })
                .OnSuccess(async _ => await _releaseService.RemoveDataFiles(releaseId, originalFileId));
        }

        private Task<Either<ActionResult, Unit>> InvalidateDataBlockCachedResults(
            DataBlockReplacementPlanViewModel plan, Guid releaseId)
        {
            return _cacheKeyService
                .CreateCacheKeyForDataBlock(releaseId, plan.Id)
                .OnSuccessVoid(_cacheService.DeleteItem);
        }

        private static Guid ReplacementPlanOriginalId(TargetableReplacementViewModel plan)
        {
            return plan.Id;
        }

        private static Guid ReplacementPlanTargetId(TargetableReplacementViewModel plan)
        {
            return plan.TargetValue;
        }

        private class ReplacementSubjectMeta
        {
            public Dictionary<string, Filter> Filters { get; set; }
            public Dictionary<string, Indicator> Indicators { get; set; }
            public IList<Location> Locations { get; set; }
            public IList<(int Year, TimeIdentifier TimeIdentifier)> TimePeriods { get; set; }
        }
    }
}

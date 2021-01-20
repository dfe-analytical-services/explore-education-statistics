﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using GovUk.Education.ExploreEducationStatistics.Admin.Services.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Admin.Services.Interfaces.Security;
using GovUk.Education.ExploreEducationStatistics.Admin.ViewModels;
using GovUk.Education.ExploreEducationStatistics.Common.Extensions;
using GovUk.Education.ExploreEducationStatistics.Common.Model;
using GovUk.Education.ExploreEducationStatistics.Common.Model.Chart;
using GovUk.Education.ExploreEducationStatistics.Common.Model.Data;
using GovUk.Education.ExploreEducationStatistics.Common.Model.Data.Query;
using GovUk.Education.ExploreEducationStatistics.Common.Services.Interfaces.Security;
using GovUk.Education.ExploreEducationStatistics.Common.Utils;
using GovUk.Education.ExploreEducationStatistics.Content.Model;
using GovUk.Education.ExploreEducationStatistics.Content.Model.Database;
using GovUk.Education.ExploreEducationStatistics.Data.Model;
using GovUk.Education.ExploreEducationStatistics.Data.Model.Database;
using GovUk.Education.ExploreEducationStatistics.Data.Model.Services.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Data.Services;
using GovUk.Education.ExploreEducationStatistics.Data.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using static GovUk.Education.ExploreEducationStatistics.Admin.Validators.ValidationErrorMessages;
using static GovUk.Education.ExploreEducationStatistics.Admin.Validators.ValidationUtils;
using static GovUk.Education.ExploreEducationStatistics.Common.Services.EnumUtil;
using static GovUk.Education.ExploreEducationStatistics.Data.Model.Services.LocationService;
using IReleaseService = GovUk.Education.ExploreEducationStatistics.Admin.Services.Interfaces.IReleaseService;
using Unit = GovUk.Education.ExploreEducationStatistics.Common.Model.Unit;

namespace GovUk.Education.ExploreEducationStatistics.Admin.Services
{
    public class ReplacementService : IReplacementService
    {
        private readonly ContentDbContext _contentDbContext;
        private readonly StatisticsDbContext _statisticsDbContext;
        private readonly IFilterService _filterService;
        private readonly IIndicatorService _indicatorService;
        private readonly ILocationService _locationService;
        private readonly IFootnoteRepository _footnoteRepository;
        private readonly IReleaseService _releaseService;
        private readonly ITimePeriodService _timePeriodService;
        private readonly IPersistenceHelper<ContentDbContext> _contentPersistenceHelper;
        private readonly IUserService _userService;

        private static IComparer<string> LabelComparer { get; } = new LabelRelationalComparer();

        public ReplacementService(ContentDbContext contentDbContext,
            StatisticsDbContext statisticsDbContext,
            IFilterService filterService,
            IIndicatorService indicatorService,
            ILocationService locationService,
            IFootnoteRepository footnoteRepository,
            IReleaseService releaseService,
            ITimePeriodService timePeriodService,
            IPersistenceHelper<ContentDbContext> contentPersistenceHelper,
            IUserService userService)
        {
            _contentDbContext = contentDbContext;
            _statisticsDbContext = statisticsDbContext;
            _filterService = filterService;
            _indicatorService = indicatorService;
            _locationService = locationService;
            _footnoteRepository = footnoteRepository;
            _releaseService = releaseService;
            _timePeriodService = timePeriodService;
            _contentPersistenceHelper = contentPersistenceHelper;
            _userService = userService;
        }

        public async Task<Either<ActionResult, DataReplacementPlanViewModel>> GetReplacementPlan(
            Guid originalFileId,
            Guid replacementFileId)
        {
            return await CheckFileExists(originalFileId)
                .OnSuccess(async originalFile =>
                {
                    return await CheckFileExists(replacementFileId)
                        .OnSuccessDo(replacementFile =>
                            _userService.CheckCanUpdateRelease(replacementFile.Release))
                        .OnSuccessDo(replacementFile =>
                            CheckFilesAreForRelatedReleases(
                                originalFile,
                                replacementFile
                            )
                        )
                        .OnSuccess(replacementFile =>
                        {
                            var releaseId = replacementFile.ReleaseId;
                            var originalSubjectId = originalFile.SubjectId.Value;
                            var replacementSubjectId = replacementFile.SubjectId.Value;

                            var replacementSubjectMeta = GetReplacementSubjectMeta(replacementSubjectId);

                            var dataBlocks = ValidateDataBlocks(releaseId, originalSubjectId, replacementSubjectMeta);
                            var footnotes = ValidateFootnotes(releaseId, originalSubjectId, replacementSubjectMeta);

                            return new DataReplacementPlanViewModel(
                                dataBlocks,
                                footnotes,
                                originalSubjectId,
                                replacementSubjectId);
                        });
                });
        }

        public async Task<Either<ActionResult, Unit>> Replace(
            Guid originalFileId,
            Guid replacementFileId)
        {
            return await GetReplacementPlan(originalFileId, replacementFileId)
                .OnSuccess(async replacementPlan =>
                {
                    if (!replacementPlan.Valid)
                    {
                        return ValidationActionResult(ReplacementMustBeValid);
                    }

                    // This Release Id can be found on the File
                    var releaseId = (await _contentDbContext.Files
                        .FindAsync(replacementFileId)).ReleaseId;

                    await replacementPlan.DataBlocks.ForEachAsync(plan =>
                        ReplaceLinksForDataBlock(plan, replacementPlan.ReplacementSubjectId));
                    await replacementPlan.Footnotes.ForEachAsync(plan =>
                        ReplaceLinksForFootnote(plan, replacementPlan.OriginalSubjectId,
                            replacementPlan.ReplacementSubjectId));
                    await ReplaceMetaGuidance(releaseId, replacementPlan.OriginalSubjectId,
                        replacementPlan.ReplacementSubjectId);

                    await _contentDbContext.SaveChangesAsync();
                    await _statisticsDbContext.SaveChangesAsync();

                    return await RemoveOriginalSubjectAndFileFromRelease(releaseId, originalFileId, replacementFileId);
                });
        }

        private async Task<Either<ActionResult, File>> CheckFileExists(Guid id)
        {
            return await _contentPersistenceHelper.CheckEntityExists<File>(
                    id,
                    q => q.Include(f => f.Release)
                )
                .OnSuccess<ActionResult, File, File>(file =>
                    {
                        if (file.Type != FileType.Data)
                        {
                            return ValidationActionResult(ReplacementFileTypesMustBeData);
                        }

                        return file;
                    }
                );
        }

        private async Task<Either<ActionResult, Unit>> CheckFilesAreForRelatedReleases(
            File originalFile,
            File replacementFile)
        {
            // Get the latest Release referencing the original File
            var originalReleaseId = await _contentDbContext.ReleaseFiles
                .GroupJoin(_contentDbContext.Releases, releaseFile => releaseFile.ReleaseId,
                    newerVersion => newerVersion.PreviousVersionId,
                    (releaseFile, newerVersionGroup) => new {releaseFile, newerVersionGroup})
                .SelectMany(tuple => tuple.newerVersionGroup.DefaultIfEmpty(),
                    (tuple, newerVersion) => new {tuple.releaseFile, newerVersion})
                .Where(tuple =>
                    tuple.releaseFile.FileId == originalFile.Id
                    && tuple.newerVersion == null)
                .Select(tuple => tuple.releaseFile.ReleaseId)
                .SingleAsync();

            // Check the replacement is for the same Release
            if (replacementFile.ReleaseId != originalReleaseId)
            {
                return new NotFoundResult();
            }

            return Unit.Instance;
        }

        private ReplacementSubjectMeta GetReplacementSubjectMeta(Guid subjectId)
        {
            var filtersIncludingItems = _filterService.GetFiltersIncludingItems(subjectId)
                .ToList();

            var filters = filtersIncludingItems
                .ToDictionary(filter => filter.Name, filter => filter);

            var indicators = _indicatorService.GetIndicators(subjectId)
                .ToDictionary(filterItem => filterItem.Name, filterItem => filterItem);

            var observationalUnits = _locationService.GetObservationalUnits(subjectId);

            var timePeriods = _timePeriodService.GetTimePeriods(subjectId);

            return new ReplacementSubjectMeta
            {
                Filters = filters,
                Indicators = indicators,
                ObservationalUnits = observationalUnits,
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
            var filters = ValidateFiltersForDataBlock(dataBlock, replacementSubjectMeta);
            var indicatorGroups = ValidateIndicatorGroupsForDataBlock(dataBlock, replacementSubjectMeta);
            var locations = ValidateLocationsForDataBlock(dataBlock, replacementSubjectMeta);
            var timePeriods = ValidateTimePeriodsForDataBlock(dataBlock, replacementSubjectMeta);

            return new DataBlockReplacementPlanViewModel(dataBlock.Id,
                dataBlock.Name,
                filters,
                indicatorGroups,
                locations,
                timePeriods);
        }

        private List<FootnoteReplacementPlanViewModel> ValidateFootnotes(Guid releaseId, Guid subjectId,
            ReplacementSubjectMeta replacementSubjectMeta)
        {
            return _footnoteRepository.GetFootnotes(releaseId, subjectId)
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

        private static Dictionary<Guid, IndicatorGroupReplacementViewModel> ValidateIndicatorGroupsForFootnote(Footnote footnote,
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
                        indicators: group.Select(indicator => ValidateIndicatorForReplacement(indicator, replacementSubjectMeta))
                    )
                );
        }

        private Dictionary<Guid, FilterReplacementViewModel> ValidateFiltersForDataBlock(
            DataBlock dataBlock,
            ReplacementSubjectMeta replacementSubjectMeta)
        {
            return _statisticsDbContext.FilterItem
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

        private Dictionary<Guid, IndicatorGroupReplacementViewModel> ValidateIndicatorGroupsForDataBlock(DataBlock dataBlock,
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
            return GetEnumValues<GeographicLevel>()
                .Where(geographicLevel => !IgnoredLevels.Contains(geographicLevel))
                .ToDictionary(geographicLevel => geographicLevel.ToString(),
                    geographicLevel =>
                        ValidateLocationLevelForReplacement(dataBlock.Query.Locations,
                            geographicLevel,
                            replacementSubjectMeta))
                .Filter(pair => pair.Value.Any);
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

        private static  FilterGroupReplacementViewModel ValidateFilterGroupForReplacement(
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

        private LocationReplacementViewModel ValidateLocationLevelForReplacement(
            LocationQuery locationQuery,
            GeographicLevel geographicLevel,
            ReplacementSubjectMeta replacementSubjectMeta)
        {
            var queryProperty = typeof(LocationQuery).GetProperty(geographicLevel.ToString());
            if (queryProperty == null || queryProperty.GetMethod == null)
            {
                throw new ArgumentException(
                    $"{nameof(locationQuery)} does not have a property {geographicLevel.ToString()} with get method");
            }

            var originalCodes = (
                queryProperty.GetMethod.Invoke(locationQuery, new object[] { }) as IEnumerable<string> ?? new List<string>()
            ).ToList();

            if (!originalCodes.Any())
            {
                return new LocationReplacementViewModel(
                    label: geographicLevel.GetEnumLabel(),
                    observationalUnits: new List<ObservationalUnitReplacementViewModel>()
                );
            }

            var locations = _locationService.GetObservationalUnits(geographicLevel, originalCodes);
            var replacementLocations = replacementSubjectMeta.ObservationalUnits
                .GetValueOrDefault(geographicLevel)
                ?.ToDictionary(location => location.Code) ?? new Dictionary<string, IObservationalUnit>();

            return new LocationReplacementViewModel(
                label: geographicLevel.GetEnumLabel(),
                observationalUnits: locations
                    .Select(location => ValidateLocationForReplacement(location, replacementLocations))
                    .OrderBy(location => location.Label, LabelComparer)
            );
        }

        private static ObservationalUnitReplacementViewModel ValidateLocationForReplacement(
            IObservationalUnit location,
            Dictionary<string, IObservationalUnit> replacementLocations)
        {
            return new ObservationalUnitReplacementViewModel(
                label: location.Name,
                code: location.Code,
                target: replacementLocations.GetValueOrDefault(location.Code)?.Code ?? string.Empty
            );
        }

        private static Filter FindReplacementFilter(ReplacementSubjectMeta replacementSubjectMeta,
            string filterName)
        {
            return replacementSubjectMeta.Filters.GetValueOrDefault(filterName);
        }

        private static FilterGroup FindReplacementFilterGroup(ReplacementSubjectMeta replacementSubjectMeta,
            string filterName,
            string filterGroupLabel)
        {
            var replacementFilter = FindReplacementFilter(replacementSubjectMeta, filterName);
            return replacementFilter?.FilterGroups.SingleOrDefault(filterGroup =>
                filterGroup.Label == filterGroupLabel);
        }

        private static FilterItem FindReplacementFilterItem(ReplacementSubjectMeta replacementSubjectMeta,
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

        private async Task ReplaceLinksForDataBlock(DataBlockReplacementPlanViewModel replacementPlan,
            Guid replacementSubjectId)
        {
            var dataBlock = await _contentDbContext.ContentBlocks
                .OfType<DataBlock>()
                .SingleAsync(block => block.Id == replacementPlan.Id);

            _contentDbContext.Update(dataBlock);

            dataBlock.Query.SubjectId = replacementSubjectId;
            ReplaceDataBlockQueryFilters(replacementPlan, dataBlock);
            ReplaceDataBlockQueryIndicators(replacementPlan, dataBlock);

            var filterItemTargets = replacementPlan.Filters
                .SelectMany(filter =>
                    filter.Value.Groups.SelectMany(group => group.Value.Filters))
                .ToDictionary(plan => plan.Id, plan => plan.TargetValue);
            var indicatorTargets = replacementPlan.IndicatorGroups
                .SelectMany(group => group.Value.Indicators)
                .ToDictionary(plan => plan.Id, plan => plan.TargetValue);

            ReplaceDataBlockTableHeaders(filterItemTargets, indicatorTargets, dataBlock);
            ReplaceDataBlockCharts(filterItemTargets, indicatorTargets, dataBlock);
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

        private static void ReplaceDataBlockTableHeaders(
            Dictionary<Guid, Guid> filterItemTargets,
            Dictionary<Guid, Guid> indicatorTargets,
            DataBlock dataBlock)
        {
            var tableHeaders = dataBlock.Table.TableHeaders;

            ReplaceDataBlockTableHeaders(
                tableHeaders.Columns.FilterByType(TableHeaderType.Filter), dataBlock, filterItemTargets);
            ReplaceDataBlockTableHeaders(
                tableHeaders.Columns.FilterByType(TableHeaderType.Indicator), dataBlock, indicatorTargets);

            ReplaceDataBlockTableHeaders(
                tableHeaders.Rows.FilterByType(TableHeaderType.Filter), dataBlock, filterItemTargets);
            ReplaceDataBlockTableHeaders(
                tableHeaders.Rows.FilterByType(TableHeaderType.Indicator), dataBlock, indicatorTargets);

            foreach (var group in tableHeaders.ColumnGroups)
            {
                ReplaceDataBlockTableHeaders(
                    group.FilterByType(TableHeaderType.Filter), dataBlock, filterItemTargets);

                ReplaceDataBlockTableHeaders(
                    group.FilterByType(TableHeaderType.Indicator), dataBlock, indicatorTargets);
            }

            foreach (var group in tableHeaders.RowGroups)
            {
                ReplaceDataBlockTableHeaders(
                    group.FilterByType(TableHeaderType.Filter), dataBlock, filterItemTargets);

                ReplaceDataBlockTableHeaders(
                    group.FilterByType(TableHeaderType.Indicator), dataBlock, indicatorTargets);
            }
        }

        private static void ReplaceDataBlockTableHeaders(List<TableHeader> tableHeaders, DataBlock dataBlock,
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
            Dictionary<Guid, Guid> filterItemTargets,
            Dictionary<Guid, Guid> indicatorTargets,
            DataBlock dataBlock)
        {
            dataBlock.Charts.ForEach(
                chart =>
                {
                    ReplaceChartMajorAxisDataSets(filterItemTargets, indicatorTargets, dataBlock, chart);
                    ReplaceChartLegendDataSets(filterItemTargets, indicatorTargets, dataBlock, chart);
                }
            );
        }

        private static void ReplaceChartLegendDataSets(
            Dictionary<Guid, Guid> filterItemTargets,
            Dictionary<Guid, Guid> indicatorTargets,
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


                    if (!dataSet.Indicator.HasValue)
                    {
                        return;
                    }

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
            );
        }

        private static void ReplaceChartMajorAxisDataSets(
            Dictionary<Guid, Guid> filterItemTargets,
            Dictionary<Guid, Guid> indicatorTargets,
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
                }
            );
        }

        private async Task ReplaceLinksForFootnote(FootnoteReplacementPlanViewModel replacementPlan,
            Guid originalSubjectId,
            Guid replacementSubjectId)
        {
            await ReplaceFootnoteSubject(replacementPlan.Id, originalSubjectId, replacementSubjectId);

            await replacementPlan.Filters.ForEachAsync(async plan =>
                await ReplaceFootnoteFilter(replacementPlan.Id, plan));

            await replacementPlan.FilterGroups.ForEachAsync(async plan =>
                await ReplaceFootnoteFilterGroup(replacementPlan.Id, plan));

            await replacementPlan.FilterItems.ForEachAsync(async plan =>
                await ReplaceFootnoteFilterItem(replacementPlan.Id, plan));

            await replacementPlan.IndicatorGroups
                .SelectMany(group => group.Value.Indicators)
                .ForEachAsync(async plan =>
                    await ReplaceIndicatorFootnote(replacementPlan.Id, plan));
        }

        private async Task ReplaceFootnoteSubject(Guid footnoteId, Guid originalSubjectId, Guid replacementSubjectId)
        {
            var subjectFootnote = await _statisticsDbContext.SubjectFootnote.Where(f =>
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
            var filterFootnote = await _statisticsDbContext.FilterFootnote.SingleAsync(f =>
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
            var filterGroupFootnote = await _statisticsDbContext.FilterGroupFootnote.SingleAsync(f =>
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
            var filterItemFootnote = await _statisticsDbContext.FilterItemFootnote.SingleAsync(f =>
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
            var indicatorFootnote = await _statisticsDbContext.IndicatorFootnote.SingleAsync(f =>
                f.FootnoteId == footnoteId && f.IndicatorId == plan.Id
            );

            _statisticsDbContext.Remove(indicatorFootnote);
            await _statisticsDbContext.IndicatorFootnote.AddAsync(new IndicatorFootnote
            {
                FootnoteId = footnoteId,
                IndicatorId = plan.TargetValue
            });
        }

        private async Task ReplaceMetaGuidance(
            Guid releaseId,
            Guid originalSubject,
            Guid replacementSubject)
        {
            var originalReleaseSubject = await _statisticsDbContext.ReleaseSubject
                .Where(rs => rs.ReleaseId == releaseId &&
                             rs.SubjectId == originalSubject)
                .FirstAsync();

            var replacementReleaseSubject = await _statisticsDbContext.ReleaseSubject
                .Where(rs => rs.ReleaseId == releaseId &&
                             rs.SubjectId == replacementSubject)
                .FirstAsync();

            _statisticsDbContext.Update(replacementReleaseSubject);
            replacementReleaseSubject.MetaGuidance = originalReleaseSubject.MetaGuidance;
        }

        private async Task<Either<ActionResult, Unit>> RemoveOriginalSubjectAndFileFromRelease(
            Guid releaseId,
            Guid originalFileId,
            Guid replacementFileId)
        {
            // First, unlink the original file from the replacement before removing it.
            // Ordinarily, removing a file from a Release deletes any associated replacement
            // so that there's no possibility of abandoned replacements being orphaned from their original files.
            return await CheckFileExists(originalFileId)
                .OnSuccessVoid(async originalFile =>
                {
                    await CheckFileExists(replacementFileId)
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

        private class ReplacementSubjectMeta
        {
            public Dictionary<string, Filter> Filters { get; set; }
            public Dictionary<string, Indicator> Indicators { get; set; }
            public Dictionary<GeographicLevel, IEnumerable<IObservationalUnit>> ObservationalUnits { get; set; }
            public IEnumerable<(int Year, TimeIdentifier TimeIdentifier)> TimePeriods { get; set; }
        }
    }
}
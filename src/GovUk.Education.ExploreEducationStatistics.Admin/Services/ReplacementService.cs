#nullable enable
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using GovUk.Education.ExploreEducationStatistics.Admin.Services.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Admin.Services.Interfaces.Cache;
using GovUk.Education.ExploreEducationStatistics.Admin.Services.Interfaces.Public.Data;
using GovUk.Education.ExploreEducationStatistics.Admin.Services.Interfaces.Security;
using GovUk.Education.ExploreEducationStatistics.Admin.ViewModels;
using GovUk.Education.ExploreEducationStatistics.Admin.ViewModels.Public.Data;
using GovUk.Education.ExploreEducationStatistics.Common.Extensions;
using GovUk.Education.ExploreEducationStatistics.Common.Model;
using GovUk.Education.ExploreEducationStatistics.Common.Model.Chart;
using GovUk.Education.ExploreEducationStatistics.Common.Model.Data;
using GovUk.Education.ExploreEducationStatistics.Common.Options;
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
using GovUk.Education.ExploreEducationStatistics.Public.Data.Model;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using static GovUk.Education.ExploreEducationStatistics.Admin.Validators.ValidationErrorMessages;
using static GovUk.Education.ExploreEducationStatistics.Admin.Validators.ValidationUtils;
using IReleaseVersionService = GovUk.Education.ExploreEducationStatistics.Admin.Services.Interfaces.IReleaseVersionService;
using Unit = GovUk.Education.ExploreEducationStatistics.Common.Model.Unit;

namespace GovUk.Education.ExploreEducationStatistics.Admin.Services
{
    public class ReplacementService(
        ContentDbContext contentDbContext,
        StatisticsDbContext statisticsDbContext,
        IFilterRepository filterRepository,
        IIndicatorRepository indicatorRepository,
        IIndicatorGroupRepository indicatorGroupRepository,
        ILocationRepository locationRepository,
        IFootnoteRepository footnoteRepository,
        IReleaseVersionService releaseVersionService,
        IDataSetVersionService dataSetVersionService,
        ITimePeriodService timePeriodService,
        IUserService userService,
        ICacheKeyService cacheKeyService,
        IPrivateBlobCacheService privateCacheService,
        IDataSetVersionMappingService dataSetVersionMappingService,
        IOptions<FeatureFlagsOptions> featureFlags)
        : IReplacementService
    {
        private static IComparer<string> LabelComparer { get; } = new LabelRelationalComparer();

        public async Task<Either<ActionResult, DataReplacementPlanViewModel>> GetReplacementPlan(
            Guid releaseVersionId,
            Guid originalFileId,
            CancellationToken cancellationToken = default)
        {
            return await contentDbContext.ReleaseVersions
                .FirstOrNotFoundAsync(rv => rv.Id == releaseVersionId, cancellationToken: cancellationToken)
                .OnSuccess(userService.CheckCanUpdateReleaseVersion)
                .OnSuccess(() => CheckLinkedOriginalAndReplacementReleaseFilesExist(
                        releaseVersionId: releaseVersionId,
                        originalFileId: originalFileId))
                .OnSuccess(async tuple =>
                {
                    var originalReleaseFile = tuple.originalReleaseFile;
                    var replacementReleaseFile = tuple.replacementReleaseFile;

                    return await GenerateReplacementPlan(
                        originalReleaseFile: originalReleaseFile,
                        replacementReleaseFile: replacementReleaseFile,
                        cancellationToken: cancellationToken);
                });
        }
        
        private async Task<ReplaceApiDataSetVersionPlanViewModel?> GetApiVersionPlanViewModel(
            DataSetVersion replacementApiDataSetVersion,
            CancellationToken cancellationToken)
        {
            var apiDataSetVersionPlan = new ReplaceApiDataSetVersionPlanViewModel
            {
                DataSetId = replacementApiDataSetVersion.DataSetId,
                DataSetTitle = replacementApiDataSetVersion.DataSet.Title,
                Id = replacementApiDataSetVersion.Id,
                Version = replacementApiDataSetVersion.PublicVersion,
                Status = replacementApiDataSetVersion.Status,
                Valid = false,
            };

            if (!featureFlags.Value.EnableReplacementOfPublicApiDataSets)
            { 
                return apiDataSetVersionPlan;
            }
 
            var mappingStatus =  await dataSetVersionMappingService.GetMappingStatus(replacementApiDataSetVersion.Id, cancellationToken);
            var isPatch = DataSetVersionNumber.TryParse(apiDataSetVersionPlan.Version, out var number) && number.Patch > 0;

            // If no mapping is found and the api version status is DRAFT, this data set version was deleted and recreated (& no mapping was necessary)
            // `completeStatusResult` is used for when we are replacing a draft release file (not an amendment) and therefore the mapping is complete/not applicable.
            var completeStatusResult = new MappingStatusViewModel
            {
                FiltersComplete = true,
                FiltersHaveMajorChange = false,
                LocationsComplete = true,
                LocationsHaveMajorChange = false,
                HasDeletionChanges = false
            };

            return apiDataSetVersionPlan with
            {
                MappingStatus = mappingStatus
                                ?? (apiDataSetVersionPlan.ReadyToPublish
                                    ? completeStatusResult
                                    : null),// If no mapping is found, this data set version was deleted and recreated (& no mapping was necessary)
                Valid = (isPatch
                            ? mappingStatus is { IsMajorVersionUpdate: false } && apiDataSetVersionPlan.ReadyToPublish
                            : apiDataSetVersionPlan.ReadyToPublish)
                        || (mappingStatus is null && apiDataSetVersionPlan.ReadyToPublish) // Data set version was deleted and recreated (as opposed to as a patch increment of a previous data set version)
            };
        }

        public async Task<Either<ActionResult, Unit>> Replace(
            Guid releaseVersionId,
            Guid originalFileId,
            CancellationToken cancellationToken)
        {
            return await contentDbContext.ReleaseVersions
                .FirstOrNotFoundAsync(rv => rv.Id == releaseVersionId,
                    cancellationToken: cancellationToken)
                .OnSuccess(userService.CheckCanUpdateReleaseVersion)
                .OnSuccess(() => CheckLinkedOriginalAndReplacementReleaseFilesExist(
                    releaseVersionId: releaseVersionId,
                    originalFileId: originalFileId))
                .OnSuccessCombineWith(async releaseFiles =>
                {
                    var originalReleaseFile = releaseFiles.originalReleaseFile;
                    var replacementReleaseFile = releaseFiles.replacementReleaseFile;

                    return await GenerateReplacementPlan(
                        originalReleaseFile: originalReleaseFile,
                        replacementReleaseFile: replacementReleaseFile,
                        cancellationToken: cancellationToken);
                })
                .OnSuccessDo(releaseFilesAndPlan =>
                {
                    var ((_, _), plan) = releaseFilesAndPlan;
                    if (!plan.Valid)
                    {
                        return new Either<ActionResult, Tuple<
                            (ReleaseFile, ReleaseFile), DataReplacementPlanViewModel>>(
                            ValidationActionResult(ReplacementMustBeValid));
                    }

                    return releaseFilesAndPlan;
                })
                .OnSuccess(async releaseFilesAndPlan =>
                {
                    var ((originalReleaseFile, replacementReleaseFile), plan) = releaseFilesAndPlan;

                    var originalSubjectId = plan.OriginalSubjectId;
                    var replacementSubjectId = plan.ReplacementSubjectId;

                    await plan.DataBlocks
                        .ToAsyncEnumerable()
                        .ForEachAwaitAsync(async dataBlockPlan =>
                        {
                            await InvalidateDataBlockCachedResults(dataBlockPlan, releaseVersionId);
                            await ReplaceLinksForDataBlock(dataBlockPlan, replacementSubjectId);
                        });

                    await plan.Footnotes
                        .ToAsyncEnumerable()
                        .ForEachAwaitAsync(footnotePlan =>
                            ReplaceLinksForFootnote(footnotePlan, originalSubjectId, replacementSubjectId));

                    replacementReleaseFile.FilterSequence =
                        await ReplaceFilterSequence(originalReleaseFile, replacementReleaseFile);
                    replacementReleaseFile.IndicatorSequence =
                        await ReplaceIndicatorSequence(originalReleaseFile, replacementReleaseFile);

                    replacementReleaseFile.Summary = originalReleaseFile.Summary; // Set Data guidance

                    // To remove original, we first unlink the files. If we don't do this,
                    // ReleaseVersionService.RemoveDataFiles will remove the replacement file as well!
                    originalReleaseFile.File.ReplacedById = null;
                    replacementReleaseFile.File.ReplacingId = null;

                    await contentDbContext.SaveChangesAsync(cancellationToken);
                    await statisticsDbContext.SaveChangesAsync(cancellationToken); // For footnotes

                    return await releaseVersionService.RemoveDataFiles(
                        releaseVersionId: releaseVersionId,
                        fileId: originalReleaseFile.FileId);
                });
        }

        private async Task<Either<ActionResult, DataReplacementPlanViewModel>> GenerateReplacementPlan(
            ReleaseFile originalReleaseFile,
            ReleaseFile replacementReleaseFile,
            CancellationToken cancellationToken)
        {
            return await
            GetLinkedDataSetVersion(
                    featureFlags.Value.EnableReplacementOfPublicApiDataSets
                        ? replacementReleaseFile
                        : originalReleaseFile,
                    cancellationToken)
                .OnSuccess(async replacementApiDataSetVersion =>
                {
                    var replacementSubjectId = replacementReleaseFile.File.SubjectId!.Value;
                    var originalSubjectId = originalReleaseFile.File.SubjectId!.Value;

                    var replacementSubjectMeta = await GetReplacementSubjectMeta(replacementSubjectId);

                    var releaseVersionId = replacementReleaseFile.ReleaseVersionId;

                    var dataBlocks = ValidateDataBlocks(
                        releaseVersionId: releaseVersionId,
                        subjectId: originalSubjectId,
                        replacementSubjectMeta);
                    var footnotes = await ValidateFootnotes(
                        releaseVersionId: releaseVersionId,
                        subjectId: originalSubjectId,
                        replacementSubjectMeta);

                    var apiDataSetVersionPlan = replacementApiDataSetVersion is null
                        ? null
                        : await GetApiVersionPlanViewModel(replacementApiDataSetVersion, cancellationToken);

                    return new DataReplacementPlanViewModel
                    {
                        DataBlocks = dataBlocks,
                        Footnotes = footnotes,
                        ApiDataSetVersionPlan = apiDataSetVersionPlan,
                        OriginalSubjectId = originalSubjectId,
                        ReplacementSubjectId = replacementSubjectId,
                    };
                });
        }

        private async Task<Either<ActionResult, DataSetVersion?>> GetLinkedDataSetVersion(
            ReleaseFile releaseFile,
            CancellationToken cancellationToken = default)
        {
            if (releaseFile.PublicApiDataSetId is null)
            {
                return (DataSetVersion)null!;
            }

            return await dataSetVersionService.GetDataSetVersion(
                    releaseFile.PublicApiDataSetId.Value,
                    releaseFile.PublicApiDataSetVersion!,
                    cancellationToken)
                .OnSuccess(dsv => (DataSetVersion?)dsv)
                .OnFailureDo(_ => throw new InvalidOperationException(
                    $"API data set version could not be found. Data set ID: '{releaseFile.PublicApiDataSetId}', version: '{releaseFile.PublicApiDataSetVersion}'"));
        }

        private async Task<Either<ActionResult, (ReleaseFile originalReleaseFile, ReleaseFile replacementReleaseFile)>>
            CheckLinkedOriginalAndReplacementReleaseFilesExist(Guid releaseVersionId, Guid originalFileId)
        {
            return await contentDbContext.ReleaseFiles
                .Include(rf => rf.File)
                .FirstOrNotFoundAsync(originalReleaseFile =>
                    originalReleaseFile.ReleaseVersionId == releaseVersionId
                    && originalReleaseFile.FileId == originalFileId
                    && originalReleaseFile.File.Type == FileType.Data
                    && originalReleaseFile.File.ReplacedById != null)
                .OnSuccessCombineWith(async originalReleaseFile =>
                    await contentDbContext.ReleaseFiles
                        .Include(rf => rf.File)
                        .FirstOrNotFoundAsync(replacementReleaseFile =>
                            replacementReleaseFile.ReleaseVersionId == releaseVersionId
                            && replacementReleaseFile.FileId == originalReleaseFile.File.ReplacedById
                            && replacementReleaseFile.File.Type == FileType.Data
                            && originalReleaseFile.FileId == replacementReleaseFile.File.ReplacingId))
                .OnSuccess(releaseFiles => releaseFiles.ToValueTuple());
        }

        private async Task<ReplacementSubjectMeta> GetReplacementSubjectMeta(Guid subjectId)
        {
            var filtersIncludingItems = await filterRepository.GetFiltersIncludingItems(subjectId);

            var filters = filtersIncludingItems
                .ToDictionary(filter => filter.Name, filter => filter);

            var indicators = indicatorRepository.GetIndicators(subjectId)
                .ToDictionary(indicator => indicator.Name, indicator => indicator);

            var locations = await locationRepository.GetDistinctForSubject(subjectId);

            var timePeriods = await timePeriodService.GetTimePeriods(subjectId);

            return new ReplacementSubjectMeta
            {
                Filters = filters,
                Indicators = indicators,
                Locations = locations,
                TimePeriods = timePeriods
            };
        }

        private List<DataBlockReplacementPlanViewModel> ValidateDataBlocks(Guid releaseVersionId,
            Guid subjectId,
            ReplacementSubjectMeta replacementSubjectMeta)
        {
            return contentDbContext
                .ContentBlocks
                .Where(block => block.ReleaseVersionId == releaseVersionId)
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

        private async Task<List<FootnoteReplacementPlanViewModel>> ValidateFootnotes(Guid releaseVersionId,
            Guid subjectId,
            ReplacementSubjectMeta replacementSubjectMeta)
        {
            var footnotes = await footnoteRepository.GetFootnotes(releaseVersionId: releaseVersionId,
                subjectId: subjectId);
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
                .GetFilterItemIds();

            var existingFilterNames = statisticsDbContext
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

            return new FilterReplacementViewModel(
                filter.Id,
                target: null,  // null because this a new filter, therefore not replacing any existing filter
                filter.Label,
                filter.Name,
                filterGroupReplacementViewModels);
        }

        private Dictionary<Guid, FilterReplacementViewModel> ValidateFiltersForDataBlock(
            DataBlock dataBlock,
            ReplacementSubjectMeta replacementSubjectMeta)
        {
            return statisticsDbContext.FilterItem
                .AsQueryable()
                .Where(filterItem => dataBlock.Query.GetFilterItemIds().Contains(filterItem.Id))
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
                            target: FindReplacementFilter(replacementSubjectMeta, filter.Key.Name)?.Id,
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
            return statisticsDbContext.Indicator
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
            return statisticsDbContext.Location
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
                    dataBlock.Query.TimePeriod!.StartYear,
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
            var dataBlock = await contentDbContext.ContentBlocks
                .AsQueryable()
                .OfType<DataBlock>()
                .SingleAsync(block => block.Id == replacementPlan.Id);

            contentDbContext.Update(dataBlock);

            dataBlock.Query.SubjectId = replacementSubjectId;

            var filterItemTargets = replacementPlan.Filters
                .SelectMany(filter =>
                    filter.Value.Groups.SelectMany(group => group.Value.Filters))
                .ToDictionary(ReplacementPlanOriginalId, ReplacementPlanTargetId);

            ReplaceDataBlockQueryFilters(filterItemTargets, dataBlock);

            var filterTargets = replacementPlan.Filters
                .Where(plan => plan.Value.Target != null)
                .ToDictionary(plan => plan.Value.Id, plan => plan.Value.Target!.Value);

            ReplaceDataBlockQueryFilterHierarchiesOptions(filterTargets, filterItemTargets, dataBlock);
            ReplaceDataBlockQueryIndicators(replacementPlan, dataBlock);
            ReplaceDataBlockQueryLocations(replacementPlan, dataBlock);

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

        private static void ReplaceDataBlockQueryFilters(
            Dictionary<Guid, Guid> filterItemTargets,
            DataBlock dataBlock)
        {
            var originalFilterItemIds = dataBlock.Query.GetNonHierarchicalFilterItemIds();
            dataBlock.Query.Filters = originalFilterItemIds
                .Select(originalFilterItemId => filterItemTargets[originalFilterItemId])
                .ToList();
        }

        private static void ReplaceDataBlockQueryFilterHierarchiesOptions(
            Dictionary<Guid, Guid> filterTargets,
            Dictionary<Guid, Guid> filterItemTargets,
            DataBlock dataBlock)
        {
            var originalFilterHierarchiesOptions = dataBlock.Query.FilterHierarchiesOptions;
            if (originalFilterHierarchiesOptions is null || originalFilterHierarchiesOptions.Count == 0)
            {
                dataBlock.Query.FilterHierarchiesOptions = null;
                return;
            }

            var newFilterHierarchiesOptions = new List<FilterHierarchyOptions>();

            foreach (var originalHierarchyOptions in originalFilterHierarchiesOptions) // for each filter hierarchy
            {
                var originalOptions = originalHierarchyOptions.Options;
                var newOptions = originalOptions
                        .Select(originalOption =>
                            originalOption // a filter hierarchy option is a list of filterItemIds, one per filter/tier
                                .Select(originalFilterItemId => filterItemTargets[originalFilterItemId])
                                .ToList())
                        .ToList();

                var replacementFilterId = filterTargets[originalHierarchyOptions.LeafFilterId];
                newFilterHierarchiesOptions.Add(new FilterHierarchyOptions
                {
                    LeafFilterId = replacementFilterId,
                    Options = newOptions,
                });
            }

            dataBlock.Query.FilterHierarchiesOptions = newFilterHierarchiesOptions;
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
                    if (chart is MapChart mapChart)
                    {
                        ReplaceMapChartDataSetConfigs(
                            filterItemTargets: filterItemTargets,
                            indicatorTargets: indicatorTargets,
                            locationTargets: locationTargets,
                            dataBlock,
                            mapChart);
                    }
                }
            );
        }

        private static void ReplaceMapChartDataSetConfigs(
            IReadOnlyDictionary<Guid, Guid> filterItemTargets,
            IReadOnlyDictionary<Guid, Guid> indicatorTargets,
            IReadOnlyDictionary<Guid, Guid> locationTargets,
            DataBlock dataBlock,
            MapChart mapChart)
        {
            mapChart.Map.DataSetConfigs.ForEach(
                dataSetConfig =>
                {
                    var dataSet = dataSetConfig.DataSet;

                    dataSet.Filters = dataSet.Filters.Select(
                        filter =>
                        {
                            if (filterItemTargets.TryGetValue(filter, out var targetFilterId))
                            {
                                return targetFilterId;
                            }

                            throw new InvalidOperationException(
                                $"Expected target replacement value for dataBlock {dataBlock.Id} chart data set config filter: {filter}"
                            );
                        }
                    ).ToList();

                    if (dataSet.Indicator.HasValue
                        && indicatorTargets.TryGetValue(dataSet.Indicator.Value, out var targetIndicatorId))
                    {
                        dataSet.Indicator = targetIndicatorId;
                    }
                    else
                    {
                        throw new InvalidOperationException(
                            $"Expected target replacement value for dataBlock {dataBlock.Id} chart data set config indicator: {dataSet.Indicator}"
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
                                $"Expected target replacement value for dataBlock {dataBlock.Id} chart data set config location: {dataSet.Location.Value}"
                            );
                        }
                    }
                });
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

                    if (dataSet.Indicator.HasValue
                        && indicatorTargets.TryGetValue(dataSet.Indicator.Value, out var targetIndicatorId))
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
            var subjectFootnote = await statisticsDbContext.SubjectFootnote
                .AsQueryable()
                .Where(f =>
                    f.FootnoteId == footnoteId && f.SubjectId == originalSubjectId).SingleOrDefaultAsync();

            if (subjectFootnote != null)
            {
                statisticsDbContext.Remove(subjectFootnote);
                await statisticsDbContext.SubjectFootnote.AddAsync(new SubjectFootnote
                {
                    FootnoteId = footnoteId,
                    SubjectId = replacementSubjectId
                });
            }
        }

        private async Task ReplaceFootnoteFilter(Guid footnoteId, TargetableReplacementViewModel plan)
        {
            var filterFootnote = await statisticsDbContext.FilterFootnote
                .AsQueryable()
                .SingleAsync(f =>
                    f.FootnoteId == footnoteId && f.FilterId == plan.Id
                );

            statisticsDbContext.Remove(filterFootnote);
            await statisticsDbContext.FilterFootnote.AddAsync(new FilterFootnote
            {
                FootnoteId = footnoteId,
                FilterId = plan.TargetValue
            });
        }

        private async Task ReplaceFootnoteFilterGroup(Guid footnoteId, TargetableReplacementViewModel plan)
        {
            var filterGroupFootnote = await statisticsDbContext.FilterGroupFootnote
                .AsQueryable()
                .SingleAsync(f =>
                    f.FootnoteId == footnoteId && f.FilterGroupId == plan.Id
                );

            statisticsDbContext.Remove(filterGroupFootnote);
            await statisticsDbContext.FilterGroupFootnote.AddAsync(new FilterGroupFootnote
            {
                FootnoteId = footnoteId,
                FilterGroupId = plan.TargetValue
            });
        }

        private async Task ReplaceFootnoteFilterItem(Guid footnoteId, TargetableReplacementViewModel plan)
        {
            var filterItemFootnote = await statisticsDbContext.FilterItemFootnote
                .AsQueryable()
                .SingleAsync(f =>
                    f.FootnoteId == footnoteId && f.FilterItemId == plan.Id
                );

            statisticsDbContext.Remove(filterItemFootnote);
            await statisticsDbContext.FilterItemFootnote.AddAsync(new FilterItemFootnote
            {
                FootnoteId = footnoteId,
                FilterItemId = plan.TargetValue
            });
        }

        private async Task ReplaceIndicatorFootnote(Guid footnoteId, TargetableReplacementViewModel plan)
        {
            var indicatorFootnote = await statisticsDbContext.IndicatorFootnote
                .AsQueryable()
                .SingleAsync(f =>
                    f.FootnoteId == footnoteId && f.IndicatorId == plan.Id
                );

            statisticsDbContext.Remove(indicatorFootnote);
            await statisticsDbContext.IndicatorFootnote.AddAsync(new IndicatorFootnote
            {
                FootnoteId = footnoteId,
                IndicatorId = plan.TargetValue
            });
        }

        private async Task<List<FilterSequenceEntry>?> ReplaceFilterSequence(ReleaseFile originalReleaseFile,
            ReleaseFile replacementReleaseFile)
        {
            // If the sequence is undefined then leave it so we continue to fallback to ordering by label alphabetically
            if (originalReleaseFile.FilterSequence == null)
            {
                return null;
            }

            var originalFilters =
                await filterRepository.GetFiltersIncludingItems(originalReleaseFile.File.SubjectId!.Value);
            var replacementFilters =
                await filterRepository.GetFiltersIncludingItems(replacementReleaseFile.File.SubjectId!.Value);

            return ReplacementServiceHelper.ReplaceFilterSequence(
                originalFilters: originalFilters,
                replacementFilters: replacementFilters,
                originalReleaseFile);
        }

        private async Task<List<IndicatorGroupSequenceEntry>?> ReplaceIndicatorSequence(
            ReleaseFile originalReleaseFile,
            ReleaseFile replacementReleaseFile)
        {
            // If the sequence is undefined then leave it so we continue to fallback to ordering by label alphabetically
            if (originalReleaseFile.IndicatorSequence == null)
            {
                return null;
            }

            var originalIndicatorGroups =
                await indicatorGroupRepository.GetIndicatorGroups(originalReleaseFile.File.SubjectId!.Value);
            var replacementIndicatorGroups =
                await indicatorGroupRepository.GetIndicatorGroups(replacementReleaseFile.File.SubjectId!.Value);

            return ReplacementServiceHelper.ReplaceIndicatorSequence(
                originalIndicatorGroups: originalIndicatorGroups,
                replacementIndicatorGroups: replacementIndicatorGroups,
                originalReleaseFile);
        }

        private Task<Either<ActionResult, Unit>> InvalidateDataBlockCachedResults(
            DataBlockReplacementPlanViewModel plan, Guid releaseVersionId)
        {
            return cacheKeyService
                .CreateCacheKeyForDataBlock(releaseVersionId: releaseVersionId,
                    dataBlockId: plan.Id)
                .OnSuccessVoid(privateCacheService.DeleteItemAsync);
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
            public Dictionary<string, Filter> Filters { get; set; } = new();
            public Dictionary<string, Indicator> Indicators { get; set; } = new();
            public IList<Location> Locations { get; set; } = null!;
            public IList<(int Year, TimeIdentifier TimeIdentifier)> TimePeriods { get; set; } = null!;
        }
    }
}

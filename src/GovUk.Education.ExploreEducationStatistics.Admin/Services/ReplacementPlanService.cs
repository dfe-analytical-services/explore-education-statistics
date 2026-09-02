#nullable enable
using GovUk.Education.ExploreEducationStatistics.Admin.Services.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Admin.Services.Interfaces.Public.Data;
using GovUk.Education.ExploreEducationStatistics.Admin.Services.Interfaces.Security;
using GovUk.Education.ExploreEducationStatistics.Admin.ViewModels;
using GovUk.Education.ExploreEducationStatistics.Admin.ViewModels.Public.Data;
using GovUk.Education.ExploreEducationStatistics.Common.Extensions;
using GovUk.Education.ExploreEducationStatistics.Common.Model;
using GovUk.Education.ExploreEducationStatistics.Common.Services.Interfaces.Security;
using GovUk.Education.ExploreEducationStatistics.Common.Utils;
using GovUk.Education.ExploreEducationStatistics.Content.Model;
using GovUk.Education.ExploreEducationStatistics.Content.Model.Database;
using GovUk.Education.ExploreEducationStatistics.Content.Model.Repository.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Data.Model;
using GovUk.Education.ExploreEducationStatistics.Data.Model.Database;
using GovUk.Education.ExploreEducationStatistics.Data.Model.Repository.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Data.Services;
using GovUk.Education.ExploreEducationStatistics.Data.Services.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Public.Data.Model;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace GovUk.Education.ExploreEducationStatistics.Admin.Services;

public class ReplacementPlanService(
    ContentDbContext contentDbContext,
    StatisticsDbContext statisticsDbContext,
    IFootnoteRepository footnoteRepository,
    ILocationRepository locationRepository,
    IDataSetVersionService dataSetVersionService,
    ITimePeriodService timePeriodService,
    IUserService userService,
    IDataSetVersionMappingService apiDataSetVersionMappingService,
    IReleaseFileRepository releaseFileRepository
) : IReplacementPlanService
{
    private static IComparer<string> LabelComparer { get; } = new LabelRelationalComparer();

    public async Task<Either<ActionResult, DataReplacementPlanViewModel>> GetReplacementPlan(
        Guid releaseVersionId,
        Guid originalFileId,
        CancellationToken cancellationToken = default
    )
    {
        return await contentDbContext
            .ReleaseVersions.Include(rv => rv.Release)
            .FirstOrNotFoundAsync(rv => rv.Id == releaseVersionId, cancellationToken: cancellationToken)
            .OnSuccess(userService.CheckCanUpdateReleaseVersion)
            .OnSuccess(() =>
                releaseFileRepository.CheckLinkedOriginalAndReplacementReleaseFilesExist(
                    releaseVersionId: releaseVersionId,
                    originalFileId: originalFileId
                )
            )
            .OnSuccess(async releaseFiles =>
                await GenerateReplacementPlan(
                    originalReleaseFile: releaseFiles.originalReleaseFile,
                    replacementReleaseFile: releaseFiles.replacementReleaseFile,
                    cancellationToken: cancellationToken
                )
            );
    }

    private async Task<ReplaceApiDataSetVersionPlanViewModel?> GetApiVersionPlanViewModel(
        DataSetVersion replacementApiDataSetVersion,
        CancellationToken cancellationToken
    )
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

        var apiMappingStatus = await apiDataSetVersionMappingService.GetMappingStatus(
            replacementApiDataSetVersion.Id,
            cancellationToken
        );
        var isPatch = DataSetVersionNumber.TryParse(apiDataSetVersionPlan.Version, out var number) && number.Patch > 0;

        // If no mapping is found and the API version status is DRAFT, this data set version was deleted and recreated (& no mapping was necessary)
        // `completeStatusResult` is used for when we are replacing a draft release file (not an amendment) and therefore the mapping is complete/not applicable.
        var completeStatusResult = new MappingStatusViewModel
        {
            FiltersComplete = true,
            FiltersHaveMajorChange = false,
            LocationsComplete = true,
            LocationsHaveMajorChange = false,
            IndicatorsComplete = true,
            IndicatorsHaveMajorChange = false,
            HasDeletionChanges = false,
        };

        return apiDataSetVersionPlan with
        {
            MappingStatus = apiMappingStatus ?? (apiDataSetVersionPlan.ReadyToPublish ? completeStatusResult : null), // If no mapping is found, this data set version was deleted and recreated (& no mapping was necessary)
            Valid =
                (
                    isPatch
                        ? apiMappingStatus is { IsMajorVersionUpdate: false } && apiDataSetVersionPlan.ReadyToPublish
                        : apiDataSetVersionPlan.ReadyToPublish
                ) || (apiMappingStatus is null && apiDataSetVersionPlan.ReadyToPublish), // Data set version was deleted and recreated (as opposed to as a patch increment of a previous data set version)
        };
    }

    public async Task<Either<ActionResult, DataReplacementPlanViewModel>> GenerateReplacementPlan(
        ReleaseFile originalReleaseFile,
        ReleaseFile replacementReleaseFile,
        CancellationToken cancellationToken
    )
    {
        return await GetLinkedDataSetVersion(replacementReleaseFile, cancellationToken)
            .OnSuccess(async replacementApiDataSetVersion =>
            {
                var originalSubjectId = originalReleaseFile.File.SubjectId!.Value;
                var replacementSubjectId = replacementReleaseFile.File.SubjectId!.Value;

                var replacementTimePeriods = await timePeriodService.GetTimePeriods(replacementSubjectId);

                var mapping = await contentDbContext.DataSetMappings.SingleAsync(
                    map =>
                        map.OriginalDataFileId == originalReleaseFile.FileId
                        && map.ReplacementDataFileId == replacementReleaseFile.FileId,
                    cancellationToken
                );

                var releaseVersionId = replacementReleaseFile.ReleaseVersionId;

                var dataBlocks = ValidateDataBlocks(
                    releaseVersionId: releaseVersionId,
                    subjectId: originalSubjectId,
                    mapping,
                    replacementTimePeriods
                );
                var footnotes = await ValidateFootnotes(
                    releaseVersionId: releaseVersionId,
                    subjectId: originalSubjectId,
                    mapping
                );

                var apiDataSetVersionPlan = replacementApiDataSetVersion is null
                    ? null
                    : await GetApiVersionPlanViewModel(replacementApiDataSetVersion, cancellationToken);

                var replacementFilters = await statisticsDbContext
                    .Filter.AsNoTracking()
                    .Include(f => f.FilterGroups)
                        .ThenInclude(g => g.FilterItems)
                    .Where(f => f.SubjectId == replacementSubjectId)
                    .ToListAsync(cancellationToken);

                var replacementIndicators = await statisticsDbContext
                    .Indicator.AsNoTracking()
                    .Include(i => i.IndicatorGroup)
                    .Where(i => i.IndicatorGroup.SubjectId == replacementSubjectId)
                    .ToListAsync(cancellationToken);

                var replacementLocations = (
                    await locationRepository.GetDistinctForSubject(replacementSubjectId)
                ).ToList();

                var mappingPlan = ReplacementPlanMappingViewModel.FromModel(
                    mapping,
                    replacementFilters,
                    replacementIndicators,
                    replacementLocations
                );

                return new DataReplacementPlanViewModel
                {
                    DataBlocks = dataBlocks,
                    Footnotes = footnotes,
                    ApiDataSetVersionPlan = apiDataSetVersionPlan,
                    OriginalSubjectId = originalSubjectId,
                    ReplacementSubjectId = replacementSubjectId,
                    Mapping = mappingPlan,
                };
            });
    }

    public async Task<bool> HasValidReplacementPlan(
        ReleaseFile originalReleaseFile,
        ReleaseFile replacementReleaseFile,
        CancellationToken cancellationToken = default
    )
    {
        var result = await GenerateReplacementPlan(originalReleaseFile, replacementReleaseFile, cancellationToken);

        return result.IsRight && result.Right.Valid;
    }

    private async Task<Either<ActionResult, DataSetVersion?>> GetLinkedDataSetVersion(
        ReleaseFile releaseFile,
        CancellationToken cancellationToken = default
    )
    {
        if (releaseFile.PublicApiDataSetId is null)
        {
            return (DataSetVersion)null!;
        }

        return await dataSetVersionService
            .GetDataSetVersion(
                releaseFile.PublicApiDataSetId.Value,
                releaseFile.PublicApiDataSetVersion!,
                cancellationToken
            )
            .OnSuccess(dsv => (DataSetVersion?)dsv)
            .OnFailureDo(_ =>
                throw new InvalidOperationException(
                    $"API data set version could not be found. Data set ID: '{releaseFile.PublicApiDataSetId}', version: '{releaseFile.PublicApiDataSetVersion}'"
                )
            );
    }

    private List<DataBlockReplacementPlanViewModel> ValidateDataBlocks(
        Guid releaseVersionId,
        Guid subjectId,
        DataSetMapping mapping,
        IList<(int Year, TimeIdentifier TimeIdentifier)> replacementTimePeriods
    )
    {
        return contentDbContext
            .DataBlockVersions.AsNoTracking()
            .Where(dataBlockVersion => dataBlockVersion.ReleaseVersionId == releaseVersionId)
            .ToList()
            .Where(dataBlockVersion => dataBlockVersion.Query.SubjectId == subjectId)
            .Select(dataBlockVersion =>
            {
                var existingFilters = ValidateFiltersForDataBlock(
                    dataBlockVersion.Query.GetFilterItemIds().ToHashSet(),
                    mapping
                );
                var indicatorGroups = CreateIndicatorGroupReplacementViewModel(
                    dataBlockVersion.Query.Indicators.ToHashSet(),
                    mapping
                );
                var locations = ValidateLocationsForDataBlock(dataBlockVersion.Query.LocationIds.ToHashSet(), mapping);
                var timePeriods = ValidateTimePeriodsForDataBlock(dataBlockVersion, replacementTimePeriods);

                return new DataBlockReplacementPlanViewModel(
                    dataBlockVersion.Id,
                    dataBlockVersion.Name,
                    existingFilters,
                    indicatorGroups,
                    locations,
                    timePeriods
                );
            })
            .ToList();
    }

    private async Task<List<FootnoteReplacementPlanViewModel>> ValidateFootnotes(
        Guid releaseVersionId,
        Guid subjectId,
        DataSetMapping mapping
    )
    {
        var footnotes = await footnoteRepository.GetFootnotes(releaseVersionId: releaseVersionId, subjectId: subjectId);
        return footnotes.Select(footnote => ValidateFootnote(footnote, mapping)).ToList();
    }

    private static FootnoteReplacementPlanViewModel ValidateFootnote(Footnote footnote, DataSetMapping mapping)
    {
        var filters = ValidateFiltersForFootnote(footnote, mapping);
        var filterGroups = ValidateFilterGroupsForFootnote(footnote, mapping);
        var filterItems = ValidateFilterItemsForFootnote(footnote, mapping);
        var indicatorGroups = CreateIndicatorGroupReplacementViewModel(
            footnote.Indicators.Select(indFootnote => indFootnote.IndicatorId).ToHashSet(),
            mapping
        );

        return new FootnoteReplacementPlanViewModel(
            footnote.Id,
            footnote.Content,
            filters,
            filterGroups,
            filterItems,
            indicatorGroups
        );
    }

    private static List<FootnoteFilterReplacementViewModel> ValidateFiltersForFootnote(
        Footnote footnote,
        DataSetMapping mapping
    )
    {
        var footnoteFilterIds = footnote.Filters.Select(f => f.FilterId).ToHashSet();

        return mapping
            .FilterMappings.Values.Where(filterMap => footnoteFilterIds.Contains(filterMap.OriginalId))
            .Select(filterMap => new FootnoteFilterReplacementViewModel(
                id: filterMap.OriginalId,
                label: filterMap.OriginalLabel,
                target: filterMap.ReplacementId
            ))
            .OrderBy(f => f.Label, LabelComparer)
            .ToList();
    }

    private static List<FootnoteFilterGroupReplacementViewModel> ValidateFilterGroupsForFootnote(
        Footnote footnote,
        DataSetMapping mapping
    )
    {
        var footnoteFilterGroupIds = footnote.FilterGroups.Select(g => g.FilterGroupId).ToHashSet();
        return mapping
            .FilterMappings.Values.SelectMany(
                filterMap => filterMap.FilterGroupMappings.Values,
                (filterMap, groupMap) => new { Filter = filterMap, FilterGroup = groupMap }
            )
            .Where(pair => footnoteFilterGroupIds.Contains(pair.FilterGroup.OriginalId))
            .Select(pair => new FootnoteFilterGroupReplacementViewModel(
                id: pair.FilterGroup.OriginalId,
                label: pair.FilterGroup.OriginalLabel,
                filterId: pair.Filter.OriginalId,
                filterLabel: pair.Filter.OriginalLabel,
                target: pair.FilterGroup.ReplacementId
            ))
            .OrderBy(f => f.Label, LabelComparer)
            .ToList();
    }

    private static List<FootnoteFilterItemReplacementViewModel> ValidateFilterItemsForFootnote(
        Footnote footnote,
        DataSetMapping mapping
    )
    {
        var footnoteFilterItemIds = footnote.FilterItems.Select(f => f.FilterItemId).ToHashSet();
        return mapping
            .FilterMappings.Values.SelectMany(
                filterMap => filterMap.FilterGroupMappings.Values,
                (filterMap, groupMap) => new { Filter = filterMap, FilterGroup = groupMap }
            )
            .SelectMany(
                pair => pair.FilterGroup.FilterItemMappings.Values,
                (pair, itemMap) =>
                    new
                    {
                        pair.Filter,
                        pair.FilterGroup,
                        FilterItem = itemMap,
                    }
            )
            .Where(trio => footnoteFilterItemIds.Contains(trio.FilterItem.OriginalId))
            .Select(trio => new FootnoteFilterItemReplacementViewModel(
                id: trio.FilterItem.OriginalId,
                label: trio.FilterItem.OriginalLabel,
                filterId: trio.Filter.OriginalId,
                filterLabel: trio.Filter.OriginalLabel,
                filterGroupId: trio.FilterGroup.OriginalId,
                filterGroupLabel: trio.FilterGroup.OriginalLabel,
                target: trio.FilterItem.ReplacementId
            ))
            .ToList();
    }

    // Returns only items (and their parent group/filter) that are included in the data block
    private static Dictionary<Guid, FilterReplacementViewModel> ValidateFiltersForDataBlock(
        HashSet<Guid> dataBlockFilterItemIds,
        DataSetMapping mapping
    )
    {
        var result = new Dictionary<Guid, FilterReplacementViewModel>();

        foreach (var filterMap in mapping.FilterMappings.Values)
        {
            var groupViewModels = ValidateFilterGroupsForDataBlock(
                dataBlockFilterItemIds,
                filterMap.FilterGroupMappings
            );

            if (groupViewModels.Count > 0)
            {
                result.Add(
                    filterMap.OriginalId,
                    new FilterReplacementViewModel(
                        id: filterMap.OriginalId,
                        target: filterMap.ReplacementId,
                        label: filterMap.OriginalLabel,
                        name: filterMap.OriginalColumnName,
                        groups: groupViewModels
                    )
                );
            }
        }

        return result;
    }

    private static Dictionary<Guid, FilterGroupReplacementViewModel> ValidateFilterGroupsForDataBlock(
        HashSet<Guid> dataBlockFilterItemIds,
        Dictionary<Guid, FilterGroupMapping> filterGroupMappings
    )
    {
        var groupViewModels = new Dictionary<Guid, FilterGroupReplacementViewModel>();

        foreach (var groupMap in filterGroupMappings)
        {
            var itemViewModels = ValidateFilterItemsForDataBlock(
                dataBlockFilterItemIds,
                groupMap.Value.FilterItemMappings
            );

            if (itemViewModels.Count > 0)
            {
                groupViewModels.Add(
                    groupMap.Value.OriginalId,
                    new FilterGroupReplacementViewModel(
                        groupMap.Value.OriginalId,
                        groupMap.Value.OriginalLabel,
                        groupMap.Value.ReplacementId,
                        itemViewModels
                    )
                );
            }
        }

        return groupViewModels;
    }

    private static List<FilterItemReplacementViewModel> ValidateFilterItemsForDataBlock(
        HashSet<Guid> dataBlockFilterItemIds,
        Dictionary<Guid, FilterItemMapping> filterItemMappings
    )
    {
        var itemViewModels = new List<FilterItemReplacementViewModel>();

        foreach (var itemMap in filterItemMappings)
        {
            if (dataBlockFilterItemIds.Contains(itemMap.Value.OriginalId))
            {
                itemViewModels.Add(
                    new FilterItemReplacementViewModel(
                        itemMap.Value.OriginalId,
                        itemMap.Value.OriginalLabel,
                        itemMap.Value.ReplacementId
                    )
                );
            }
        }

        return itemViewModels;
    }

    private static Dictionary<string, LocationReplacementViewModel> ValidateLocationsForDataBlock(
        HashSet<Guid> dataBlockLocationIds,
        DataSetMapping mapping
    )
    {
        return mapping
            .LocationMappings.Values.Where(map => dataBlockLocationIds.Contains(map.OriginalId))
            .GroupBy(map => map.OriginalGeographicLevel)
            .ToDictionary(
                group => group.Key.ToString(),
                group => new LocationReplacementViewModel(
                    label: group.Key.ToString(),
                    locationAttributes: group
                        .Select(map => new LocationAttributeReplacementViewModel(
                            id: map.OriginalId,
                            code: map.OriginalCode,
                            label: map.OriginalName,
                            target: map.ReplacementId
                        ))
                        .OrderBy(location => location.Label, LabelComparer)
                )
            );
    }

    private static TimePeriodRangeReplacementViewModel ValidateTimePeriodsForDataBlock(
        DataBlockVersion dataBlockVersion,
        IList<(int Year, TimeIdentifier TimeIdentifier)> replacementTimePeriods
    )
    {
        return new TimePeriodRangeReplacementViewModel(
            start: ValidateTimePeriodForReplacement(
                dataBlockVersion.Query.TimePeriod!.StartYear,
                dataBlockVersion.Query.TimePeriod.StartCode,
                replacementTimePeriods
            ),
            end: ValidateTimePeriodForReplacement(
                dataBlockVersion.Query.TimePeriod.EndYear,
                dataBlockVersion.Query.TimePeriod.EndCode,
                replacementTimePeriods
            )
        );
    }

    private static TimePeriodReplacementViewModel ValidateTimePeriodForReplacement(
        int year,
        TimeIdentifier code,
        IList<(int Year, TimeIdentifier TimeIdentifier)> replacementTimePeriods
    )
    {
        return new TimePeriodReplacementViewModel(
            year: year,
            code: code,
            valid: replacementTimePeriods.Contains((year, code))
        );
    }

    private static Dictionary<Guid, IndicatorGroupReplacementViewModel> CreateIndicatorGroupReplacementViewModel(
        HashSet<Guid> indicatorIds,
        DataSetMapping mapping
    )
    {
        return mapping
            .IndicatorMappings.Values.Where(map => indicatorIds.Contains(map.OriginalId))
            .GroupBy(map => new { GroupId = map.OriginalGroupId, GroupLabel = map.OriginalGroupLabel })
            .OrderBy(group => group.Key.GroupLabel, LabelComparer)
            .ToDictionary(
                group => group.Key.GroupId,
                group => new IndicatorGroupReplacementViewModel(
                    id: group.Key.GroupId,
                    label: group.Key.GroupLabel,
                    indicators: group
                        .OrderBy(map => map.OriginalLabel, LabelComparer)
                        .Select(map => new IndicatorReplacementViewModel(
                            id: map.OriginalId,
                            name: map.OriginalColumnName,
                            label: map.OriginalLabel,
                            target: map.ReplacementId
                        ))
                )
            );
    }
}

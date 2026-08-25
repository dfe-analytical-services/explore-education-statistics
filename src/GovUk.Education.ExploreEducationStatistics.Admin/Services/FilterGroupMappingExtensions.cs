#nullable enable
using GovUk.Education.ExploreEducationStatistics.Admin.Requests;
using GovUk.Education.ExploreEducationStatistics.Common.Extensions;
using GovUk.Education.ExploreEducationStatistics.Common.ViewModels;
using GovUk.Education.ExploreEducationStatistics.Content.Model;
using GovUk.Education.ExploreEducationStatistics.Data.Model;

namespace GovUk.Education.ExploreEducationStatistics.Admin.Services;

internal static class FilterGroupMappingExtensions
{
    public static (FilterGroupMapping? FilterGroupMapping, ErrorViewModel? Error) UpdateFilterGroupMapping(
        this DataSetMapping dataSetMapping,
        Dictionary<Guid, (FilterMapping FilterMap, FilterGroupMapping GroupMap)> originalGroupIdToGroupMap,
        IReadOnlyList<Filter> replacementFilters,
        Guid originalId,
        Guid? newReplacementId = null
    )
    {
        if (!originalGroupIdToGroupMap.TryGetValue(originalId, out var pair))
        {
            return (
                null,
                new ErrorViewModel
                {
                    Path =
                        $"{nameof(FilterMappingUpdatesRequest.FilterGroupUpdates)}.{nameof(MappingUpdateRequest.OriginalId)}",
                    Code = "FilterGroupMatchingOriginalIdNotFound",
                    Message =
                        $"Could not find filter group mapping matching original id \"{originalId}\". DataSetMapping.Id: {dataSetMapping.Id}",
                }
            );
        }

        var (filterMapping, groupMapping) = pair;

        if (groupMapping.Status == MapStatus.ParentNotMapped)
        {
            return (
                null,
                new ErrorViewModel
                {
                    Path =
                        $"{nameof(FilterMappingUpdatesRequest.FilterGroupUpdates)}.{nameof(MappingUpdateRequest.OriginalId)}",
                    Code = "FilterGroupParentNotMapped",
                    Message =
                        $"Cannot map a group whose parent filter isn't mapped. OriginalId: \"{groupMapping.OriginalId}\"",
                }
            );
        }

        if (groupMapping.ReplacementId == newReplacementId && groupMapping.Status == MapStatus.ManuallySet)
        {
            return (groupMapping, null); // already set, nothing to do
        }

        var replacementFilterGroups = replacementFilters
            .Where(f => f.Id == filterMapping.ReplacementId)
            .SelectMany(f => f.FilterGroups)
            .ToList();

        if (
            newReplacementId != null
            && !dataSetMapping.IsFilterGroupCandidateAvailable(
                groupMapping,
                replacementFilterGroups,
                newReplacementId.Value
            )
        )
        {
            return (
                null,
                new ErrorViewModel
                {
                    Path =
                        $"{nameof(FilterMappingUpdatesRequest.FilterGroupUpdates)}.{nameof(MappingUpdateRequest.NewReplacementId)}",
                    Code = "UnmappedFilterGroupMatchingReplacementIdNotFound",
                    Message = $"No available unmapped filter group matching replacement id \"{newReplacementId}\".",
                }
            );
        }

        var replacementGroup =
            newReplacementId == null ? null : replacementFilterGroups.Single(group => group.Id == newReplacementId);

        // mapping.Original* properties should never change
        groupMapping.ReplacementId = replacementGroup?.Id;
        groupMapping.ReplacementLabel = replacementGroup?.Label;
        groupMapping.Status = MapStatus.ManuallySet;

        groupMapping.AutoMapChildItems(replacementGroup?.FilterItems ?? []);

        return (groupMapping, null);
    }

    private static bool IsFilterGroupCandidateAvailable(
        this DataSetMapping dataSetMapping,
        FilterGroupMapping groupMapping,
        IReadOnlyList<FilterGroup> replacementFilterGroups,
        Guid candidateId
    )
    {
        var candidateGroupExists = replacementFilterGroups.Any(candidateGroup => candidateGroup.Id == candidateId);

        var alreadyClaimed = dataSetMapping
            .FilterMappings.Values.SelectMany(filterMap => filterMap.FilterGroupMappings.Values)
            .Any(other => other.OriginalId != groupMapping.OriginalId && other.ReplacementId == candidateId);

        return candidateGroupExists && !alreadyClaimed;
    }

    internal static void AutoMapChildItems(
        this FilterGroupMapping groupMapping,
        IReadOnlyList<FilterItem> replacementFilterItems
    )
    {
        if (groupMapping.ReplacementId == null)
        {
            groupMapping.FilterItemMappings.Values.ForEach(itemMapping =>
            {
                itemMapping.ReplacementId = null;
                itemMapping.ReplacementLabel = null;
                itemMapping.Status = MapStatus.ParentNotMapped;
            });
            return;
        }

        var candidateItemsByLabel = replacementFilterItems.ToDictionary(candidateItem => candidateItem.Label);

        foreach (var itemMapping in groupMapping.FilterItemMappings.Values)
        {
            if (candidateItemsByLabel.Remove(itemMapping.OriginalLabel, out var match))
            {
                itemMapping.ReplacementId = match.Id;
                itemMapping.ReplacementLabel = match.Label;
                itemMapping.Status = MapStatus.AutoSet;
            }
            else
            {
                itemMapping.ReplacementId = null;
                itemMapping.ReplacementLabel = null;
                itemMapping.Status = MapStatus.Unset;
            }
        }
    }
}

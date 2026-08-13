#nullable enable
using GovUk.Education.ExploreEducationStatistics.Admin.Requests;
using GovUk.Education.ExploreEducationStatistics.Common.ViewModels;
using GovUk.Education.ExploreEducationStatistics.Content.Model;
using GovUk.Education.ExploreEducationStatistics.Data.Model;

namespace GovUk.Education.ExploreEducationStatistics.Admin.Services;

internal static class FilterItemMappingExtensions
{
    public static (FilterItemMapping? FilterItemMapping, ErrorViewModel? Error) UpdateFilterItemMapping(
        this DataSetMapping dataSetMapping,
        Dictionary<Guid, (FilterGroupMapping FilterGroup, FilterItemMapping FilterItem)> originalItemIdToItemMap,
        List<Filter> replacementFilters,
        Guid originalId,
        Guid? newReplacementId = null
    )
    {
        if (!originalItemIdToItemMap.TryGetValue(originalId, out var pair))
        {
            return (
                null,
                new ErrorViewModel
                {
                    Path =
                        $"{nameof(FilterMappingUpdatesRequest.FilterItemUpdates)}.{nameof(MappingUpdateRequest.OriginalId)}",
                    Code = "FilterItemMatchingOriginalIdNotFound",
                    Message =
                        $"Could not find filter item mapping matching original id \"{originalId}\". DataSetMapping.Id: {dataSetMapping.Id}",
                }
            );
        }

        var (groupMapping, itemMapping) = pair;

        if (itemMapping.Status == MapStatus.ParentNotMapped)
        {
            return (
                null,
                new ErrorViewModel
                {
                    Path =
                        $"{nameof(FilterMappingUpdatesRequest.FilterItemUpdates)}.{nameof(MappingUpdateRequest.OriginalId)}",
                    Code = "FilterItemParentNotMapped",
                    Message =
                        $"Cannot map a filter item whose parent group isn't mapped. OriginalId: \"{itemMapping.OriginalId}\"",
                }
            );
        }

        if (itemMapping.ReplacementId == newReplacementId && itemMapping.Status == MapStatus.ManuallySet)
        {
            return (itemMapping, null); // it is already mapped
        }

        var replacementFilterItems =
            replacementFilters
                .SelectMany(f => f.FilterGroups)
                .Where(g => g.Id == groupMapping.ReplacementId)
                .Select(g => g.FilterItems)
                .SingleOrDefault()
            ?? [];

        if (
            newReplacementId != null
            && !dataSetMapping.IsFilterItemCandidateAvailable(
                itemMapping,
                replacementFilterItems,
                newReplacementId.Value
            )
        )
        {
            return (
                null,
                new ErrorViewModel
                {
                    Path =
                        $"{nameof(FilterMappingUpdatesRequest.FilterItemUpdates)}.{nameof(MappingUpdateRequest.NewReplacementId)}",
                    Code = "UnmappedFilterItemMatchingReplacementIdNotFound",
                    Message = $"No available unmapped filter item matching replacement id \"{newReplacementId}\".",
                }
            );
        }

        var replacementItem =
            newReplacementId == null ? null : replacementFilterItems.Single(item => item.Id == newReplacementId);

        // mapping.Original* properties should never change
        itemMapping.ReplacementId = replacementItem?.Id;
        itemMapping.ReplacementLabel = replacementItem?.Label;
        itemMapping.Status = MapStatus.ManuallySet;

        return (itemMapping, null);
    }

    private static bool IsFilterItemCandidateAvailable(
        this DataSetMapping dataSetMapping,
        FilterItemMapping itemMapping,
        List<FilterItem> replacementFilterItems,
        Guid candidateId
    )
    {
        var candidateItemExists = replacementFilterItems.Any(candidateItem => candidateItem.Id == candidateId);

        var alreadyClaimed = dataSetMapping
            .FilterMappings.Values.SelectMany(filterMap => filterMap.FilterGroupMappings.Values)
            .SelectMany(groupMap => groupMap.FilterItemMappings.Values)
            .Any(other => other.OriginalId != itemMapping.OriginalId && other.ReplacementId == candidateId);

        return candidateItemExists && !alreadyClaimed;
    }
}

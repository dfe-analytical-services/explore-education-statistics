#nullable enable
using GovUk.Education.ExploreEducationStatistics.Admin.Requests;
using GovUk.Education.ExploreEducationStatistics.Common.ViewModels;
using GovUk.Education.ExploreEducationStatistics.Content.Model;

namespace GovUk.Education.ExploreEducationStatistics.Admin.Services;

internal static class FilterItemMappingExtensions
{
    public static (FilterItemMapping? FilterItemMapping, ErrorViewModel? Error) UpdateMapping(
        this FilterItemMapping itemMapping,
        DataSetMapping dataSetMapping,
        FilterGroupMapping groupMapping,
        Guid? newReplacementId = null
    )
    {
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

        if (
            newReplacementId != null
            && !dataSetMapping.IsFilterItemCandidateAvailable(groupMapping, itemMapping, newReplacementId.Value)
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

        var replacement =
            newReplacementId == null
                ? null
                : dataSetMapping.ReplacementFilterItems.Single(item => item.Id == newReplacementId);

        // mapping.Original* properties should never change
        itemMapping.ReplacementId = replacement?.Id;
        itemMapping.ReplacementLabel = replacement?.Label;
        itemMapping.Status = MapStatus.ManuallySet;

        return (itemMapping, null);
    }

    // A candidate is available if it exists under the item's (new) parent replacement group, and isn't already
    // claimed by some other live item mapping - "claimed" just means another mapping's own ReplacementId points
    // at it; there's no separate pool of "unmapped" candidates to keep in sync.
    private static bool IsFilterItemCandidateAvailable(
        this DataSetMapping dataSetMapping,
        FilterGroupMapping groupMapping,
        FilterItemMapping itemMapping,
        Guid candidateId
    )
    {
        var candidateExists = dataSetMapping.ReplacementFilterItems.Any(candidate =>
            candidate.Id == candidateId && candidate.FilterGroupId == groupMapping.ReplacementId
        );

        var alreadyClaimed = dataSetMapping
            .FilterMappings.Values.SelectMany(filterMap => filterMap.FilterGroupMappings.Values)
            .SelectMany(groupMap => groupMap.FilterItemMappings.Values)
            .Any(other => other.OriginalId != itemMapping.OriginalId && other.ReplacementId == candidateId);

        return candidateExists && !alreadyClaimed;
    }

    internal static void ResetToParentNotMapped(this FilterItemMapping itemMapping)
    {
        itemMapping.ReplacementId = null;
        itemMapping.ReplacementLabel = null;
        itemMapping.Status = MapStatus.ParentNotMapped;
    }
}

#nullable enable
using GovUk.Education.ExploreEducationStatistics.Admin.Requests;
using GovUk.Education.ExploreEducationStatistics.Common.Extensions;
using GovUk.Education.ExploreEducationStatistics.Common.ViewModels;
using GovUk.Education.ExploreEducationStatistics.Content.Model;

namespace GovUk.Education.ExploreEducationStatistics.Admin.Services;

internal static class FilterGroupMappingExtensions
{
    public static (FilterGroupMapping? FilterGroupMapping, ErrorViewModel? Error) UpdateMapping(
        this FilterGroupMapping groupMapping,
        DataSetMapping dataSetMapping,
        FilterMapping filterMapping,
        Guid? newReplacementId = null
    )
    {
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

        if (
            newReplacementId != null
            && !dataSetMapping.IsFilterGroupCandidateAvailable(filterMapping, groupMapping, newReplacementId.Value)
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

        var replacement =
            newReplacementId == null
                ? null
                : dataSetMapping.ReplacementFilterGroups.Single(group => group.Id == newReplacementId);

        // mapping.Original* properties should never change
        groupMapping.ReplacementId = replacement?.Id;
        groupMapping.ReplacementLabel = replacement?.Label;
        groupMapping.Status = MapStatus.ManuallySet;

        // re-evaluate every child item against the (possibly new, possibly cleared) replacement group
        groupMapping.AutoMapChildItems(dataSetMapping);

        return (groupMapping, null);
    }

    // A candidate is available if it exists under the group's (new) parent replacement filter, and isn't already
    // claimed by some other live group mapping - see the equivalent method in FilterItemMappingExtensions for why
    // there's no separate "unmapped" pool to consult.
    private static bool IsFilterGroupCandidateAvailable(
        this DataSetMapping dataSetMapping,
        FilterMapping filterMapping,
        FilterGroupMapping groupMapping,
        Guid candidateId
    )
    {
        var candidateExists = dataSetMapping.ReplacementFilterGroups.Any(candidate =>
            candidate.Id == candidateId && candidate.FilterId == filterMapping.ReplacementId
        );

        var alreadyClaimed = dataSetMapping
            .FilterMappings.Values.SelectMany(filterMap => filterMap.FilterGroupMappings.Values)
            .Any(other => other.OriginalId != groupMapping.OriginalId && other.ReplacementId == candidateId);

        return candidateExists && !alreadyClaimed;
    }

    // Matches this group's children to the replacement group's children by label (greedily, one candidate per
    // child), falling back to Unset when there's no match for a given child.
    internal static void AutoMapChildItems(this FilterGroupMapping groupMapping, DataSetMapping dataSetMapping)
    {
        if (groupMapping.ReplacementId == null)
        {
            groupMapping.FilterItemMappings.Values.ForEach(itemMapping => itemMapping.ResetToParentNotMapped());
            return;
        }

        var candidatesByLabel = dataSetMapping
            .ReplacementFilterItems.Where(candidate => candidate.FilterGroupId == groupMapping.ReplacementId)
            .ToDictionary(candidate => candidate.Label);

        foreach (var itemMapping in groupMapping.FilterItemMappings.Values)
        {
            if (candidatesByLabel.Remove(itemMapping.OriginalLabel, out var match))
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

    internal static void ResetToParentNotMapped(this FilterGroupMapping groupMapping)
    {
        groupMapping.ReplacementId = null;
        groupMapping.ReplacementLabel = null;
        groupMapping.Status = MapStatus.ParentNotMapped;
        groupMapping.FilterItemMappings.Values.ForEach(itemMapping => itemMapping.ResetToParentNotMapped());
    }
}

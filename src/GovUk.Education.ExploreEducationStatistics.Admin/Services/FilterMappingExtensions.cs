#nullable enable
using GovUk.Education.ExploreEducationStatistics.Admin.Requests;
using GovUk.Education.ExploreEducationStatistics.Common.Extensions;
using GovUk.Education.ExploreEducationStatistics.Common.ViewModels;
using GovUk.Education.ExploreEducationStatistics.Content.Model;

namespace GovUk.Education.ExploreEducationStatistics.Admin.Services;

internal static class FilterMappingExtensions
{
    public static (FilterMapping? FilterMapping, ErrorViewModel? Error) UpdateMapping(
        this FilterMapping filterMapping,
        DataSetMapping dataSetMapping,
        Guid? newReplacementId = null
    )
    {
        if (filterMapping.ReplacementId == newReplacementId && filterMapping.Status == MapStatus.ManuallySet)
        {
            return (filterMapping, null); // already set, nothing to do
        }

        if (
            newReplacementId != null
            && !dataSetMapping.IsFilterCandidateAvailable(filterMapping, newReplacementId.Value)
        )
        {
            return (
                null,
                new ErrorViewModel
                {
                    Path =
                        $"{nameof(FilterMappingUpdatesRequest.FilterUpdates)}.{nameof(MappingUpdateRequest.NewReplacementId)}",
                    Code = "UnmappedFilterMatchingReplacementIdNotFound",
                    Message =
                        $"No available unmapped filter matching replacement id \"{newReplacementId}\". DataSetMapping.Id: {dataSetMapping.Id}",
                }
            );
        }

        var replacement =
            newReplacementId == null
                ? null
                : dataSetMapping.ReplacementFilters.Single(filter => filter.Id == newReplacementId);

        // mapping.Original* properties should never change
        filterMapping.ReplacementId = replacement?.Id;
        filterMapping.ReplacementColumnName = replacement?.ColumnName;
        filterMapping.ReplacementLabel = replacement?.Label;
        filterMapping.Status = MapStatus.ManuallySet;

        // re-evaluate every child group (and, transitively, every child item) against the (possibly new, possibly
        // cleared) replacement filter
        filterMapping.AutoMapChildGroups(dataSetMapping);

        return (filterMapping, null);
    }

    // A candidate is available if it exists in the replacement-side catalogue, and isn't already claimed by some
    // other live filter mapping. There's no separate "unmapped" pool to add to/remove from - availability is just
    // "does any live mapping already point at this id", computed fresh from the current mapping tree each time.
    private static bool IsFilterCandidateAvailable(
        this DataSetMapping dataSetMapping,
        FilterMapping filterMapping,
        Guid candidateId
    )
    {
        var candidateExists = dataSetMapping.ReplacementFilters.Any(candidate => candidate.Id == candidateId);

        var alreadyClaimed = dataSetMapping.FilterMappings.Values.Any(other =>
            other.OriginalId != filterMapping.OriginalId && other.ReplacementId == candidateId
        );

        return candidateExists && !alreadyClaimed;
    }

    // Matches this filter's children to the replacement filter's children by label (greedily, one candidate per
    // child), falling back to Unset when there's no match for a given child, or ParentNotMapped for all children
    // when the filter itself has no replacement.
    internal static void AutoMapChildGroups(this FilterMapping filterMapping, DataSetMapping dataSetMapping)
    {
        if (filterMapping.ReplacementId == null)
        {
            filterMapping.FilterGroupMappings.Values.ForEach(groupMapping => groupMapping.ResetToParentNotMapped());
            return;
        }

        var candidatesByLabel = dataSetMapping
            .ReplacementFilterGroups.Where(candidate => candidate.FilterId == filterMapping.ReplacementId)
            .ToDictionary(candidate => candidate.Label);

        foreach (var groupMapping in filterMapping.FilterGroupMappings.Values)
        {
            if (candidatesByLabel.Remove(groupMapping.OriginalLabel, out var match))
            {
                groupMapping.ReplacementId = match.Id;
                groupMapping.ReplacementLabel = match.Label;
                groupMapping.Status = MapStatus.AutoSet;

                groupMapping.AutoMapChildItems(dataSetMapping);
            }
            else
            {
                // NOTE: a group's own children are intentionally left untouched here (matching pre-existing
                // behaviour) - only an explicit group-level update, or the parent filter being unmapped, resets
                // a group's children.
                groupMapping.ReplacementId = null;
                groupMapping.ReplacementLabel = null;
                groupMapping.Status = MapStatus.Unset;
            }
        }
    }
}

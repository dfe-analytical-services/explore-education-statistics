#nullable enable
using GovUk.Education.ExploreEducationStatistics.Admin.Requests;
using GovUk.Education.ExploreEducationStatistics.Common.Extensions;
using GovUk.Education.ExploreEducationStatistics.Common.ViewModels;
using GovUk.Education.ExploreEducationStatistics.Content.Model;
using GovUk.Education.ExploreEducationStatistics.Data.Model;

namespace GovUk.Education.ExploreEducationStatistics.Admin.Services;

internal static class FilterMappingExtensions
{
    public static (FilterMapping? FilterMapping, ErrorViewModel? Error) UpdateFilterMapping(
        this DataSetMapping dataSetMapping,
        IReadOnlyList<Filter> replacementFilters,
        Guid originalId,
        Guid? newReplacementId = null
    )
    {
        if (!dataSetMapping.FilterMappings.TryGetValue(originalId, out var filterMapping))
        {
            return (
                null,
                new ErrorViewModel
                {
                    Path =
                        $"{nameof(FilterMappingUpdatesRequest.FilterUpdates)}.{nameof(MappingUpdateRequest.OriginalId)}",
                    Code = "FilterMatchingOriginalIdNotFound",
                    Message =
                        $"Could not find filter mapping matching original id \"{originalId}\". DataSetMapping.Id: {dataSetMapping.Id}",
                }
            );
        }

        if (filterMapping.ReplacementId == newReplacementId && filterMapping.Status == MapStatus.ManuallySet)
        {
            return (filterMapping, null); // already set, nothing to do
        }

        if (
            newReplacementId != null
            && !IsFilterCandidateAvailable(dataSetMapping, filterMapping, replacementFilters, newReplacementId.Value)
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
            newReplacementId == null ? null : replacementFilters.Single(filter => filter.Id == newReplacementId);

        // mapping.Original* properties should never change
        filterMapping.ReplacementId = replacement?.Id;
        filterMapping.ReplacementColumnName = replacement?.Name;
        filterMapping.ReplacementLabel = replacement?.Label;
        filterMapping.Status = MapStatus.ManuallySet;

        filterMapping.AutoMapChildGroups(replacement?.FilterGroups ?? []);

        return (filterMapping, null);
    }

    private static bool IsFilterCandidateAvailable(
        DataSetMapping dataSetMapping,
        FilterMapping filterMapping,
        IReadOnlyList<Filter> replacementFilters,
        Guid candidateId
    )
    {
        var candidateExists = replacementFilters.Any(candidate => candidate.Id == candidateId);

        var alreadyClaimed = dataSetMapping.FilterMappings.Values.Any(other =>
            other.OriginalId != filterMapping.OriginalId && other.ReplacementId == candidateId
        );

        return candidateExists && !alreadyClaimed;
    }

    private static void AutoMapChildGroups(this FilterMapping filterMapping, List<FilterGroup> replacementFilterGroups)
    {
        if (filterMapping.ReplacementId == null)
        {
            filterMapping.FilterGroupMappings.Values.ForEach(groupMapping =>
            {
                groupMapping.ReplacementId = null;
                groupMapping.ReplacementLabel = null;
                groupMapping.Status = MapStatus.ParentNotMapped;
                groupMapping.AutoMapChildItems([]); // Set child items to ReplacementId = null, Status = ParentNotMapped
            });
            return;
        }

        var candidateGroupsByLabel = replacementFilterGroups.ToDictionary(candidateGroup => candidateGroup.Label);

        foreach (var groupMapping in filterMapping.FilterGroupMappings.Values)
        {
            if (candidateGroupsByLabel.Remove(groupMapping.OriginalLabel, out var match))
            {
                groupMapping.ReplacementId = match.Id;
                groupMapping.ReplacementLabel = match.Label;
                groupMapping.Status = MapStatus.AutoSet;

                groupMapping.AutoMapChildItems(match.FilterItems);
            }
            else
            {
                groupMapping.ReplacementId = null;
                groupMapping.ReplacementLabel = null;
                groupMapping.Status = MapStatus.Unset;

                groupMapping.AutoMapChildItems([]); // Set child items to ReplacementId = null, Status = ParentNotMapped
            }
        }
    }
}

#nullable enable
using GovUk.Education.ExploreEducationStatistics.Admin.Requests;
using GovUk.Education.ExploreEducationStatistics.Admin.Services.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Common.Extensions;
using GovUk.Education.ExploreEducationStatistics.Common.ViewModels;
using GovUk.Education.ExploreEducationStatistics.Content.Model;
using GovUk.Education.ExploreEducationStatistics.Content.Model.Database;

namespace GovUk.Education.ExploreEducationStatistics.Admin.Services;

public class FilterGroupMappingService(
    ContentDbContext contentDbContext,
    IFilterItemMappingService filterItemMappingService
) : IFilterGroupMappingService
{
    public (FilterGroupMapping? FilterGroupMapping, ErrorViewModel? Error) UpdateFilterGroupMapping(
        DataSetMapping dataSetMapping,
        Dictionary<Guid, (FilterMapping FilterMap, FilterGroupMapping GroupMap)> originalGroupIdToGroupMap,
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

        var filterMapping = pair.FilterMap;
        var groupMapping = pair.GroupMap;

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
                        $"Cannot map a group whose parent filter isn't mapped. OriginalId: \"{originalId}\". DataSetMapping.Id: {dataSetMapping.Id}",
                }
            );
        }

        if (groupMapping.ReplacementId == newReplacementId && groupMapping.Status == MapStatus.ManuallySet)
        {
            return (groupMapping, null); // already set, nothing to do
        }

        var availableUnmappedGroup = filterMapping.UnmappedReplacementFilterGroups.SingleOrDefault(unmappedGroup =>
            unmappedGroup.Id == newReplacementId
        );

        if (newReplacementId != null && availableUnmappedGroup == null)
        {
            return (
                null,
                new ErrorViewModel
                {
                    Path =
                        $"{nameof(FilterMappingUpdatesRequest.FilterGroupUpdates)}.{nameof(MappingUpdateRequest.NewReplacementId)}",
                    Code = "UnmappedFilterGroupMatchingReplacementIdNotFound",
                    Message =
                        $"No available unmapped filter group matching replacement id \"{newReplacementId}\". DataSetMapping.Id: {dataSetMapping.Id}",
                }
            );
        }

        if (availableUnmappedGroup != null)
        {
            // remove availableUnmappedGroup from UnmappedReplacementFilterGroups as it's about to become mapped
            filterMapping.UnmappedReplacementFilterGroups.Remove(availableUnmappedGroup);
            contentDbContext.Entry(dataSetMapping).Property(x => x.FilterMappings).IsModified = true;
        }

        if (groupMapping.ReplacementId != null && groupMapping.ReplacementId != newReplacementId)
        {
            UnmapFilterGroupMapping(dataSetMapping, filterMapping, groupMapping);
        }

        // If a replacement is set, we need to automap all child groups and items
        var (filterItemMappings, unmappedReplacementItems) = filterItemMappingService.AutoMapFilterItemMappings(
            groupMapping.FilterItemMappings.Values.ToList(),
            availableUnmappedGroup?.UnmappedReplacementFilterItems
        );

        // mapping.Original* properties should never change
        groupMapping.ReplacementId = availableUnmappedGroup?.Id;
        groupMapping.ReplacementLabel = availableUnmappedGroup?.Label;
        groupMapping.Status = MapStatus.ManuallySet;

        groupMapping.FilterItemMappings = filterItemMappings;
        groupMapping.UnmappedReplacementFilterItems = unmappedReplacementItems;

        contentDbContext.Entry(dataSetMapping).Property(x => x.FilterMappings).IsModified = true;

        return (groupMapping, null);
    }

    public (
        Dictionary<Guid, FilterGroupMapping> FilterGroupMappings,
        List<UnmappedFilterGroup> UnmappedReplacementGroups
    ) AutoMapFilterGroupMappings(
        List<FilterGroupMapping> groupMappings,
        List<UnmappedFilterGroup>? unmappedReplacementGroups
    )
    {
        if (unmappedReplacementGroups == null)
        {
            // groups' parent filter replacement has been unset, so no automapping required, as the parent filter
            // would have been unmapped before this method was called, and so all groups/items would be set correctly
            // (i.e. ReplacementId == null && Status == ParentNotMapped)
            return (groupMappings.ToDictionary(groupMap => groupMap.OriginalId, groupMap => groupMap), []);
        }

        var unmappedGroupLabelToUnmappedGroupMap = unmappedReplacementGroups.ToDictionary(
            unmappedReplacementGroup => unmappedReplacementGroup.Label,
            unmappedReplacementGroup => unmappedReplacementGroup
        );

        var newGroupMappings = new Dictionary<Guid, FilterGroupMapping>(groupMappings.Count);
        foreach (var groupMapping in groupMappings)
        {
            if (
                unmappedGroupLabelToUnmappedGroupMap.Remove(
                    groupMapping.OriginalLabel,
                    out var autoMappableReplacementGroup
                )
            )
            {
                groupMapping.ReplacementId = autoMappableReplacementGroup.Id;
                groupMapping.ReplacementLabel = autoMappableReplacementGroup.Label;
                groupMapping.Status = MapStatus.AutoSet;

                var (autoMappedItems, unmappedReplacementItems) = filterItemMappingService.AutoMapFilterItemMappings(
                    groupMapping.FilterItemMappings.Values.ToList(),
                    autoMappableReplacementGroup.UnmappedReplacementFilterItems
                );
                groupMapping.FilterItemMappings = autoMappedItems;
                groupMapping.UnmappedReplacementFilterItems = unmappedReplacementItems;
            }
            else
            {
                groupMapping.ReplacementId = null;
                groupMapping.ReplacementLabel = null;
                groupMapping.Status = MapStatus.Unset;
            }

            newGroupMappings.Add(groupMapping.OriginalId, groupMapping);
        }

        // remaining replacement groups in unmappedGroupLabelToUnmappedGroupMap are unmapped
        var newUnmappedReplacementGroups = unmappedGroupLabelToUnmappedGroupMap.Values.ToList();

        return (newGroupMappings, newUnmappedReplacementGroups);
    }

    private void UnmapFilterGroupMapping(
        DataSetMapping dataSetMapping,
        FilterMapping filterMapping,
        FilterGroupMapping groupMapping
    )
    {
        if (!groupMapping.ReplacementId.HasValue)
        {
            throw new Exception(
                $"Cannot unmap replacement for filter group mapping as no replacement is mapped. FilterGroup OriginalId: {groupMapping.OriginalId}, DataSetMapping.Id: {dataSetMapping.Id}"
            );
        }

        // We need to move the preexisting mapped filter into UnmappedReplacementFilters, as it will be overwritten
        var newlyUnmappedGroup = new UnmappedFilterGroup
        {
            Id = groupMapping.ReplacementId.Value,
            Label = groupMapping.ReplacementLabel!,
            UnmappedReplacementFilterItems = groupMapping
                .FilterItemMappings.Values.Where(itemMapping => itemMapping.ReplacementId != null)
                .Select(itemMapping => new UnmappedFilterItem
                {
                    Id = itemMapping.ReplacementId!.Value,
                    Label = itemMapping.ReplacementLabel!,
                })
                .Concat(groupMapping.UnmappedReplacementFilterItems)
                .ToList(),
        };

        filterMapping.UnmappedReplacementFilterGroups.Add(newlyUnmappedGroup);
        contentDbContext.Entry(dataSetMapping).Property(x => x.UnmappedReplacementFilters).IsModified = true;

        // Now remove it from groupMapping
        groupMapping.ReplacementId = null;
        groupMapping.ReplacementLabel = null;
        groupMapping.Status = MapStatus.Unset;
        groupMapping.FilterItemMappings.Values.ForEach(itemMapping =>
        {
            itemMapping.ReplacementId = null;
            itemMapping.ReplacementLabel = null;
            itemMapping.Status = MapStatus.ParentNotMapped;
        });

        contentDbContext.Entry(dataSetMapping).Property(x => x.FilterMappings).IsModified = true;
    }
}

#nullable enable
using GovUk.Education.ExploreEducationStatistics.Admin.Requests;
using GovUk.Education.ExploreEducationStatistics.Admin.Services.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Common.ViewModels;
using GovUk.Education.ExploreEducationStatistics.Content.Model;
using GovUk.Education.ExploreEducationStatistics.Content.Model.Database;

namespace GovUk.Education.ExploreEducationStatistics.Admin.Services;

public class FilterItemMappingService(ContentDbContext contentDbContext) : IFilterItemMappingService
{
    public (FilterItemMapping? FilterItemMapping, ErrorViewModel? Error) UpdateFilterItemMapping(
        DataSetMapping dataSetMapping,
        Dictionary<Guid, (FilterGroupMapping FilterGroup, FilterItemMapping FilterItem)> originalItemIdToItemMap,
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

        var filterGroupMapping = pair.FilterGroup;
        var filterItemMapping = pair.FilterItem;

        if (filterItemMapping.Status == MapStatus.ParentNotMapped)
        {
            return (
                null,
                new ErrorViewModel
                {
                    Path =
                        $"{nameof(FilterMappingUpdatesRequest.FilterItemUpdates)}.{nameof(MappingUpdateRequest.OriginalId)}",
                    Code = "FilterItemParentNotMapped",
                    Message =
                        $"Cannot map a filter item whose parent group isn't mapped. OriginalId: \"{originalId}\". DataSetMapping.Id: {dataSetMapping.Id}",
                }
            );
        }

        if (filterItemMapping.ReplacementId == newReplacementId && filterItemMapping.Status == MapStatus.ManuallySet)
        {
            return (filterItemMapping, null); // it is already mapped
        }

        var availableUnmappedItem = filterGroupMapping.UnmappedReplacementFilterItems.SingleOrDefault(unmappedItem =>
            unmappedItem.Id == newReplacementId
        );

        if (newReplacementId != null && availableUnmappedItem == null)
        {
            return (
                null,
                new ErrorViewModel
                {
                    Path =
                        $"{nameof(FilterMappingUpdatesRequest.FilterItemUpdates)}.{nameof(MappingUpdateRequest.NewReplacementId)}",
                    Code = "UnmappedFilterItemMatchingReplacementIdNotFound",
                    Message =
                        $"No available unmapped filter item matching replacement id \"{newReplacementId}\". DataSetMapping.Id: {dataSetMapping.Id}",
                }
            );
        }

        if (availableUnmappedItem != null)
        {
            filterGroupMapping.UnmappedReplacementFilterItems.Remove(availableUnmappedItem);
            contentDbContext.Entry(dataSetMapping).Property(x => x.FilterMappings).IsModified = true;
        }

        if (filterItemMapping.ReplacementId != null && filterItemMapping.ReplacementId != newReplacementId)
        {
            // We need to move the preexisting mapped item into UnmappedReplacementFilterItems, as it will be overwritten
            var newlyUnmappedItem = new UnmappedFilterItem
            {
                Id = filterItemMapping.ReplacementId.Value,
                Label = filterItemMapping.ReplacementLabel!,
            };
            filterGroupMapping.UnmappedReplacementFilterItems.Add(newlyUnmappedItem);
            contentDbContext.Entry(dataSetMapping).Property(x => x.FilterMappings).IsModified = true;
        }

        filterItemMapping.ReplacementId = availableUnmappedItem?.Id;
        filterItemMapping.ReplacementLabel = availableUnmappedItem?.Label;
        filterItemMapping.Status = MapStatus.ManuallySet;

        contentDbContext.Entry(dataSetMapping).Property(x => x.FilterMappings).IsModified = true;

        return (filterItemMapping, null);
    }

    public (
        Dictionary<Guid, FilterItemMapping> FilterItemMappings,
        List<UnmappedFilterItem> UnmappedReplacementItems
    ) AutoMapFilterItemMappings(
        List<FilterItemMapping> itemMappings,
        List<UnmappedFilterItem>? unmappedReplacementItems
    )
    {
        if (unmappedReplacementItems == null)
        {
            // items' parent group replacement has been unset, so no automapping required, as the parent group
            // would have been unmapped before this method was called, and so all items would be set correctly
            // (i.e. ReplacementId == null && Status == ParentNotMapped)
            return (itemMappings.ToDictionary(itemMap => itemMap.OriginalId, itemMap => itemMap), []);
        }

        var unmappedItemLabelToUnmappedLabelMap = unmappedReplacementItems.ToDictionary(
            unmappedItem => unmappedItem.Label,
            unmappedItem => unmappedItem
        );

        var newItemMappings = new Dictionary<Guid, FilterItemMapping>(itemMappings.Count);
        foreach (var itemMap in itemMappings)
        {
            if (unmappedItemLabelToUnmappedLabelMap.Remove(itemMap.OriginalLabel, out var autoMappableReplacementItem))
            {
                itemMap.ReplacementId = autoMappableReplacementItem.Id;
                itemMap.ReplacementLabel = autoMappableReplacementItem.Label;
                itemMap.Status = MapStatus.AutoSet;
            }
            else
            {
                itemMap.ReplacementId = null;
                itemMap.ReplacementLabel = null;
                itemMap.Status = MapStatus.Unset;
            }

            newItemMappings.Add(itemMap.OriginalId, itemMap);
        }

        // remaining replacement items are unmapped
        var newUnmappedReplacementItems = unmappedItemLabelToUnmappedLabelMap.Values.ToList();

        return (newItemMappings, newUnmappedReplacementItems);
    }
}

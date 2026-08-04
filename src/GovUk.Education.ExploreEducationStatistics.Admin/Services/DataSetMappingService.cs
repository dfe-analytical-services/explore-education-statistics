#nullable enable
using GovUk.Education.ExploreEducationStatistics.Admin.Requests;
using GovUk.Education.ExploreEducationStatistics.Admin.Services.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Admin.Services.Interfaces.Security;
using GovUk.Education.ExploreEducationStatistics.Admin.ViewModels;
using GovUk.Education.ExploreEducationStatistics.Common.Extensions;
using GovUk.Education.ExploreEducationStatistics.Common.Model;
using GovUk.Education.ExploreEducationStatistics.Common.Services.Interfaces.Security;
using GovUk.Education.ExploreEducationStatistics.Common.ViewModels;
using GovUk.Education.ExploreEducationStatistics.Content.Model;
using GovUk.Education.ExploreEducationStatistics.Content.Model.Database;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using static GovUk.Education.ExploreEducationStatistics.Common.Validators.ValidationUtils;
using ReleaseVersion = GovUk.Education.ExploreEducationStatistics.Content.Model.ReleaseVersion;

namespace GovUk.Education.ExploreEducationStatistics.Admin.Services;

public class DataSetMappingService(ContentDbContext contentDbContext, IUserService userService) : IDataSetMappingService
{
    public async Task<Either<ActionResult, FiltersMappingDto>> UpdateFilterMappings(
        Guid releaseVersionId,
        FilterMappingUpdatesRequest request,
        CancellationToken cancellationToken = default
    )
    {
        return await contentDbContext
            .ReleaseVersions.Where(rv => rv.Id == releaseVersionId)
            .SingleOrNotFound()
            .OnSuccess(userService.CheckCanUpdateReleaseVersion)
            .OnSuccess(async releaseVersion =>
                await ValidateMapping(releaseVersion, request.OriginalDataFileId, request.ReplacementDataFileId)
            )
            .OnSuccess(async mapping =>
            {
                // Filters
                var updatedFilterMappings = request
                    .FilterUpdates.Select(filterUpdate =>
                        UpdateFilterMapping(mapping, filterUpdate.OriginalId, filterUpdate.NewReplacementId)
                    )
                    .ToList();

                var filterErrors = updatedFilterMappings
                    .Where(updated => updated.Error != null)
                    .Select(updated => updated.Error!)
                    .ToList();
                if (!filterErrors.IsNullOrEmpty())
                {
                    return new Either<ActionResult, FiltersMappingDto>(ValidationResult(filterErrors));
                }

                var filterMappingDto = new FiltersMappingDto
                {
                    Filters = updatedFilterMappings
                        .Select(updated => FilterMappingDto.FromModel(updated.FilterMapping!))
                        .ToList(),
                };

                // FilterGroups
                var originalGroupIdToGroupsMap = mapping
                    .FilterMappings.Values.SelectMany(
                        fm => fm.FilterGroupMappings.Values,
                        (fm, gm) => (FilterMap: fm, GroupMap: gm)
                    )
                    .ToDictionary(x => x.GroupMap.OriginalId);

                var updatedGroupMappings = request
                    .FilterGroupUpdates.Select(groupUpdate =>
                        UpdateFilterGroupMapping(
                            mapping,
                            originalGroupIdToGroupsMap,
                            groupUpdate.OriginalId,
                            groupUpdate.NewReplacementId
                        )
                    )
                    .ToList();

                var groupErrors = updatedGroupMappings
                    .Where(updated => updated.Error != null)
                    .Select(updated => updated.Error!)
                    .ToList();
                if (!groupErrors.IsNullOrEmpty())
                {
                    return ValidationResult(groupErrors);
                }

                filterMappingDto.FilterGroups = updatedGroupMappings
                    .Select(updated => FilterGroupMappingDto.FromModel(updated.FilterGroupMapping!))
                    .ToList();

                // FilterItems
                var originalItemIdToItem = mapping
                    .FilterMappings.Values.SelectMany(fm => fm.FilterGroupMappings.Values)
                    .SelectMany(
                        fg => fg.FilterItemMappings.Values,
                        (group, item) => (FilterGroup: group, FilterItem: item)
                    )
                    .ToDictionary(x => x.FilterItem.OriginalId);

                var updatedItemMappings = request
                    .FilterItemUpdates.Select(itemUpdate =>
                        UpdateFilterItemMapping(
                            mapping,
                            originalItemIdToItem,
                            itemUpdate.OriginalId,
                            itemUpdate.NewReplacementId
                        )
                    )
                    .ToList();

                var itemErrors = updatedItemMappings
                    .Where(updated => updated.Error != null)
                    .Select(updated => updated.Error!)
                    .ToList();
                if (!itemErrors.IsNullOrEmpty())
                {
                    return ValidationResult(itemErrors);
                }

                filterMappingDto.FilterItems = updatedItemMappings
                    .Select(updated => FilterItemMappingDto.FromModel(updated.FilterItemMapping!))
                    .ToList();

                await contentDbContext.SaveChangesAsync(cancellationToken);

                return filterMappingDto;
            });
    }

    private (FilterMapping? FilterMapping, ErrorViewModel? Error) UpdateFilterMapping(
        DataSetMapping dataSetMapping,
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

        var availableUnmappedFilter = dataSetMapping.UnmappedReplacementFilters.SingleOrDefault(unmappedFilter =>
            unmappedFilter.Id == newReplacementId
        );

        if (newReplacementId != null && availableUnmappedFilter == null)
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

        if (availableUnmappedFilter != null)
        {
            // remove availableUnmappedFilter from UnmappedReplacementFilters as it's about to become mapped
            dataSetMapping.UnmappedReplacementFilters.Remove(availableUnmappedFilter);
            contentDbContext.Entry(dataSetMapping).Property(x => x.UnmappedReplacementFilters).IsModified = true;
        }

        if (filterMapping.ReplacementId != null && filterMapping.ReplacementId != newReplacementId)
        {
            UnmapFilterMapping(dataSetMapping, filterMapping);
        }

        // If a replacement is set, we need to automap all child groups and items
        var (filterGroupMappings, unmappedReplacementGroups) = AutoMapFilterGroupMappings(
            filterMapping.FilterGroupMappings.Values.ToList(),
            availableUnmappedFilter?.UnmappedReplacementFilterGroups
        );

        // mapping.Original* properties should never change
        filterMapping.ReplacementId = availableUnmappedFilter?.Id;
        filterMapping.ReplacementColumnName = availableUnmappedFilter?.ColumnName;
        filterMapping.ReplacementLabel = availableUnmappedFilter?.Label;
        filterMapping.Status = MapStatus.ManuallySet;

        filterMapping.FilterGroupMappings = filterGroupMappings;
        filterMapping.UnmappedReplacementFilterGroups = unmappedReplacementGroups;

        contentDbContext.Entry(dataSetMapping).Property(x => x.FilterMappings).IsModified = true;

        return (filterMapping, null);
    }

    private (FilterGroupMapping? FilterGroupMapping, ErrorViewModel? Error) UpdateFilterGroupMapping(
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
        var (filterItemMappings, unmappedReplacementItems) = AutoMapFilterItemMappings(
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

    private (FilterItemMapping? FilterItemMapping, ErrorViewModel? Error) UpdateFilterItemMapping(
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

    private void UnmapFilterMapping(DataSetMapping dataSetMapping, FilterMapping filterMapping)
    {
        if (!filterMapping.ReplacementId.HasValue)
        {
            throw new Exception(
                $"Cannot unmap replacement for filterMapping as no replacement is mapped. Filter OriginalId: {filterMapping.OriginalId}. DataSetMapping.Id: {dataSetMapping.Id}"
            );
        }

        // We need to move the preexisting mapped filter into UnmappedReplacementFilters, as it will be overwritten
        // and that must include all child groups and items
        var newlyUnmappedFilter = new UnmappedFilter
        {
            Id = filterMapping.ReplacementId.Value,
            ColumnName = filterMapping.ReplacementColumnName!,
            Label = filterMapping.ReplacementLabel!,
            UnmappedReplacementFilterGroups = filterMapping
                .FilterGroupMappings.Values.Where(groupMapping => groupMapping.ReplacementId != null)
                .Select(groupMapping => new UnmappedFilterGroup
                {
                    Id = groupMapping.ReplacementId!.Value,
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
                })
                .Concat(filterMapping.UnmappedReplacementFilterGroups)
                .ToList(),
        };
        dataSetMapping.UnmappedReplacementFilters.Add(newlyUnmappedFilter);
        contentDbContext.Entry(dataSetMapping).Property(x => x.UnmappedReplacementFilters).IsModified = true;

        // Now remove it from filterMapping
        filterMapping.ReplacementId = null;
        filterMapping.ReplacementColumnName = null;
        filterMapping.ReplacementLabel = null;
        filterMapping.Status = MapStatus.Unset;
        filterMapping.UnmappedReplacementFilterGroups = [];
        filterMapping.FilterGroupMappings.Values.ForEach(groupMapping =>
        {
            groupMapping.ReplacementId = null;
            groupMapping.ReplacementLabel = null;
            groupMapping.Status = MapStatus.ParentNotMapped;
            groupMapping.UnmappedReplacementFilterItems = [];
            groupMapping.FilterItemMappings.Values.ForEach(itemMapping =>
            {
                itemMapping.ReplacementId = null;
                itemMapping.ReplacementLabel = null;
                itemMapping.Status = MapStatus.ParentNotMapped;
            });
        });
        contentDbContext.Entry(dataSetMapping).Property(x => x.FilterMappings).IsModified = true;
    }

    private (
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

                var (autoMappedItems, unmappedReplacementItems) = AutoMapFilterItemMappings(
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

    private (
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

    public async Task<Either<ActionResult, List<IndicatorMappingDto>>> UpdateIndicatorMappings(
        Guid releaseVersionId,
        IndicatorMappingUpdatesRequest request,
        CancellationToken cancellationToken = default
    )
    {
        return await contentDbContext
            .ReleaseVersions.Where(rv => rv.Id == releaseVersionId)
            .SingleOrNotFound()
            .OnSuccess(userService.CheckCanUpdateReleaseVersion)
            .OnSuccess(async releaseVersion =>
                await ValidateMapping(releaseVersion, request.OriginalDataFileId, request.ReplacementDataFileId)
            )
            .OnSuccess(async mapping =>
            {
                var updatedMappings = request
                    .Updates.Select(update =>
                        UpdateIndicatorMapping(mapping, update.OriginalId, update.NewReplacementId)
                    )
                    .ToList(); // cannot be async!

                // we still save changes from the Updates that succeeded, even if some failed
                await contentDbContext.SaveChangesAsync(cancellationToken);

                return updatedMappings
                    .OnSuccessAll()
                    .OnSuccess(_ => mapping.IndicatorMappings.Values.Select(IndicatorMappingDto.FromModel).ToList());
            });
    }

    private Either<ActionResult, IndicatorMapping> UpdateIndicatorMapping(
        DataSetMapping dataSetMapping,
        Guid originalId,
        Guid? newReplacementId = null
    )
    {
        if (!dataSetMapping.IndicatorMappings.TryGetValue(originalId, out var indicatorMapping))
        {
            return ValidationResult(
                new ErrorViewModel
                {
                    Path =
                        $"{nameof(IndicatorMappingUpdatesRequest.Updates)}.{nameof(MappingUpdateRequest.OriginalId)}",
                    Code = "IndicatorMatchingOriginalIdNotFound",
                    Message = $"Could not find indicator mapping matching original id \"{originalId}\"",
                }
            );
        }

        if (indicatorMapping.ReplacementId == newReplacementId && indicatorMapping.Status == MapStatus.ManuallySet)
        {
            return indicatorMapping; // it is already mapped, so can skip
        }

        var availableUnmappedIndicator = dataSetMapping.UnmappedReplacementIndicators.SingleOrDefault(
            unmappedIndicator => unmappedIndicator.Id == newReplacementId
        );

        if (newReplacementId != null && availableUnmappedIndicator == null)
        {
            return ValidationResult(
                new ErrorViewModel
                {
                    Path =
                        $"{nameof(IndicatorMappingUpdatesRequest.Updates)}.{nameof(MappingUpdateRequest.NewReplacementId)}",
                    Code = "UnmappedIndicatorMatchingReplacementIdNotFound",
                    Message =
                        $"No available unmapped indicator matching replacement id \"{newReplacementId}\". DataSetMapping.Id: {dataSetMapping.Id}",
                }
            );
        }

        if (availableUnmappedIndicator != null)
        {
            // remove availableUnmappedIndicator from UnmappedReplacementIndicators as it's about to become mapped
            dataSetMapping.UnmappedReplacementIndicators.Remove(availableUnmappedIndicator);
            contentDbContext.Entry(dataSetMapping).Property(x => x.UnmappedReplacementIndicators).IsModified = true;
        }

        if (indicatorMapping.ReplacementId != null && indicatorMapping.ReplacementId != newReplacementId)
        {
            // We need to move the preexisting mapped indicator into UnmappedReplacementIndicators, as it will be overwritten
            var newlyUnmappedIndicator = new UnmappedIndicator
            {
                Id = indicatorMapping.ReplacementId.Value,
                ColumnName = indicatorMapping.ReplacementColumnName!,
                Label = indicatorMapping.ReplacementLabel!,
                GroupId = indicatorMapping.ReplacementGroupId!.Value,
                GroupLabel = indicatorMapping.ReplacementGroupLabel!,
            };
            dataSetMapping.UnmappedReplacementIndicators.Add(newlyUnmappedIndicator);
            contentDbContext.Entry(dataSetMapping).Property(x => x.UnmappedReplacementIndicators).IsModified = true;
        }

        // mapping.Original* properties should never change
        indicatorMapping.ReplacementId = availableUnmappedIndicator?.Id;
        indicatorMapping.ReplacementColumnName = availableUnmappedIndicator?.ColumnName;
        indicatorMapping.ReplacementLabel = availableUnmappedIndicator?.Label;
        indicatorMapping.ReplacementGroupId = availableUnmappedIndicator?.GroupId;
        indicatorMapping.ReplacementGroupLabel = availableUnmappedIndicator?.GroupLabel;
        indicatorMapping.Status = MapStatus.ManuallySet;

        contentDbContext.Entry(dataSetMapping).Property(x => x.IndicatorMappings).IsModified = true;

        return indicatorMapping;
    }

    public async Task<Either<ActionResult, List<LocationMappingDto>>> UpdateLocationMappings(
        Guid releaseVersionId,
        LocationMappingUpdatesRequest request,
        CancellationToken cancellationToken = default
    )
    {
        return await contentDbContext
            .ReleaseVersions.Where(rv => rv.Id == releaseVersionId)
            .SingleOrNotFound()
            .OnSuccess(userService.CheckCanUpdateReleaseVersion)
            .OnSuccess(async releaseVersion =>
                await ValidateMapping(releaseVersion, request.OriginalDataFileId, request.ReplacementDataFileId)
            )
            .OnSuccess(async mapping =>
            {
                var updatedMappings = request
                    .Updates.Select(update =>
                        UpdateLocationMapping(mapping, update.OriginalId, update.NewReplacementId)
                    )
                    .ToList(); // cannot be async!

                // we still save changes from the Updates that succeeded, even if some failed
                await contentDbContext.SaveChangesAsync(cancellationToken);

                return updatedMappings
                    .OnSuccessAll()
                    .OnSuccess(_ => mapping.LocationMappings.Values.Select(LocationMappingDto.FromModel).ToList());
            });
    }

    private Either<ActionResult, LocationMapping> UpdateLocationMapping(
        DataSetMapping dataSetMapping,
        Guid originalLocationId,
        Guid? newReplacementLocationId = null
    )
    {
        if (!dataSetMapping.LocationMappings.TryGetValue(originalLocationId, out var locationMapping))
        {
            return ValidationResult(
                new ErrorViewModel
                {
                    Path = $"{nameof(LocationMappingUpdatesRequest.Updates)}.{nameof(MappingUpdateRequest.OriginalId)}",
                    Code = "LocationMatchingOriginalIdNameNotFound",
                    Message = $"Could not find location mapping matching original location id \"{originalLocationId}\"",
                }
            );
        }

        if (
            locationMapping.ReplacementId == newReplacementLocationId
            && locationMapping.Status == MapStatus.ManuallySet
        )
        {
            return locationMapping; // it is already mapped, so can skip
        }

        var availableUnmappedLocation = dataSetMapping.UnmappedReplacementLocations.SingleOrDefault(unmappedLocation =>
            unmappedLocation.Id == newReplacementLocationId
        );

        if (newReplacementLocationId != null && availableUnmappedLocation == null)
        {
            return ValidationResult(
                new ErrorViewModel
                {
                    Path =
                        $"{nameof(LocationMappingUpdatesRequest.Updates)}.{nameof(MappingUpdateRequest.NewReplacementId)}",
                    Code = "UnmappedLocationMatchingReplacementLocationIdNotFound",
                    Message = $"No available unmapped location matching replacement id \"{newReplacementLocationId}\"",
                }
            );
        }

        if (
            newReplacementLocationId != null
            && availableUnmappedLocation != null
            && availableUnmappedLocation.GeographicLevel != locationMapping.OriginalGeographicLevel
        )
        {
            return ValidationResult(
                new ErrorViewModel
                {
                    Path =
                        $"{nameof(LocationMappingUpdatesRequest.Updates)}.{nameof(MappingUpdateRequest.NewReplacementId)}",
                    Code = "UnmappedLocationHasDifferentGeographicLevelAsOriginalLocation",
                    Message =
                        $"The replacement location has a different geographic level than the original location. Replacement id: \"{newReplacementLocationId}\"",
                }
            );
        }

        if (availableUnmappedLocation != null)
        {
            // remove availableUnmappedLocation from UnmappedReplacementLocations as it's about to become mapped
            dataSetMapping.UnmappedReplacementLocations.Remove(availableUnmappedLocation);
            contentDbContext.Entry(dataSetMapping).Property(x => x.UnmappedReplacementLocations).IsModified = true;
        }

        if (locationMapping.ReplacementId != null && locationMapping.ReplacementId != newReplacementLocationId)
        {
            // We need to move the preexisting mapped location into UnmappedReplacementLocations, as it will be overwritten
            var newlyUnmappedLocation = new UnmappedLocation
            {
                Id = locationMapping.ReplacementId.Value,
                GeographicLevel = locationMapping.ReplacementGeographicLevel!.Value,
                Code = locationMapping.ReplacementCode!,
                Name = locationMapping.ReplacementName!,
            };
            dataSetMapping.UnmappedReplacementLocations.Add(newlyUnmappedLocation);
            contentDbContext.Entry(dataSetMapping).Property(x => x.UnmappedReplacementLocations).IsModified = true;
        }

        // locationMapping.Original* properties should never change
        locationMapping.ReplacementId = availableUnmappedLocation?.Id;
        locationMapping.ReplacementGeographicLevel = availableUnmappedLocation?.GeographicLevel;
        locationMapping.ReplacementCode = availableUnmappedLocation?.Code;
        locationMapping.ReplacementName = availableUnmappedLocation?.Name;
        locationMapping.Status = MapStatus.ManuallySet;

        contentDbContext.Entry(dataSetMapping).Property(x => x.LocationMappings).IsModified = true;

        return locationMapping;
    }

    private async Task<Either<ActionResult, DataSetMapping>> ValidateMapping(
        ReleaseVersion releaseVersion,
        Guid originalDataFileId,
        Guid replacementDataFileId
    )
    {
        var mapping = await contentDbContext.DataSetMappings.SingleOrDefaultAsync(map =>
            map.OriginalDataFileId == originalDataFileId && map.ReplacementDataFileId == replacementDataFileId
        );

        if (mapping == null)
        {
            return new Either<ActionResult, DataSetMapping>(new NotFoundResult());
        }

        var originalReleaseFileExists = await contentDbContext.ReleaseFiles.AnyAsync(rf =>
            rf.ReleaseVersionId == releaseVersion.Id && rf.FileId == mapping.OriginalDataFileId
        );
        if (!originalReleaseFileExists)
        {
            return ValidationResult(
                new ErrorViewModel
                {
                    Path = $"{nameof(IndicatorMappingUpdatesRequest.OriginalDataFileId)}",
                    Code = "OriginalDataFileIdNotLinkedToReleaseVersion",
                    Message = $"The original data file is not linked to the release version",
                }
            );
        }
        var replacementReleaseFileExists = await contentDbContext.ReleaseFiles.AnyAsync(rf =>
            rf.ReleaseVersionId == releaseVersion.Id && rf.FileId == mapping.ReplacementDataFileId
        );
        if (!replacementReleaseFileExists)
        {
            return ValidationResult(
                new ErrorViewModel
                {
                    Path = $"{nameof(IndicatorMappingUpdatesRequest.ReplacementDataFileId)}",
                    Code = "ReplacementDataFileIdNotLinkedToReleaseVersion",
                    Message = $"The replacement data set is not linked to the release version",
                }
            );
        }

        return mapping;
    }
}

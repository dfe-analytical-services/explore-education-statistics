#nullable enable
using GovUk.Education.ExploreEducationStatistics.Admin.Requests;
using GovUk.Education.ExploreEducationStatistics.Admin.Services.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Common.Extensions;
using GovUk.Education.ExploreEducationStatistics.Common.ViewModels;
using GovUk.Education.ExploreEducationStatistics.Content.Model;
using GovUk.Education.ExploreEducationStatistics.Content.Model.Database;

namespace GovUk.Education.ExploreEducationStatistics.Admin.Services;

public class FilterMappingService(
    ContentDbContext contentDbContext,
    IFilterGroupMappingService filterGroupMappingService
) : IFilterMappingService
{
    public (FilterMapping? FilterMapping, ErrorViewModel? Error) UpdateFilterMapping(
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
        var (filterGroupMappings, unmappedReplacementGroups) = filterGroupMappingService.AutoMapFilterGroupMappings(
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
}

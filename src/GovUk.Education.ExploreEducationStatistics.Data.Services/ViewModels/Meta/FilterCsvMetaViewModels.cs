#nullable enable
using System;
using System.Collections.Generic;
using System.Linq;
using GovUk.Education.ExploreEducationStatistics.Common.Extensions;
using GovUk.Education.ExploreEducationStatistics.Data.Model;

namespace GovUk.Education.ExploreEducationStatistics.Data.Services.ViewModels.Meta;

public record FilterCsvMetaViewModel
{
    public Guid Id { get; init; }

    public string Name { get; init; } = string.Empty;

    public string? GroupCsvColumn { get; init; }

    public IReadOnlyDictionary<Guid, FilterItemCsvMetaViewModel> Items { get; init; } =
        new Dictionary<Guid, FilterItemCsvMetaViewModel>();

    public FilterCsvMetaViewModel()
    {
    }

    public FilterCsvMetaViewModel(Filter filter)
    {
        Id = filter.Id;
        Name = filter.Name;
        GroupCsvColumn = filter.GroupCsvColumn;
        Items = filter.FilterGroups
            .SelectMany(filterGroup => filterGroup.FilterItems)
            .Select(filterItem => new FilterItemCsvMetaViewModel(filterItem))
            .ToDictionary(filterItem => filterItem.Id);
    }

    public FilterCsvMetaViewModel(FilterMetaViewModel filter)
    {
        Id = filter.Id;
        // TODO EES-3755 Remove fallback after Permalink snapshot work is complete
        Name = filter.Name ?? filter.Legend.SnakeCase();
        GroupCsvColumn = filter.GroupCsvColumn;
        Items = filter.Options
            .SelectMany(kvp =>
            {
                var (_, filterGroup) = kvp;
                return filterGroup.Options
                    .Select(filterItem => new FilterItemCsvMetaViewModel(filterItem, filterGroup.Label));
            })
            .ToDictionary(filterItem => filterItem.Id);
    }
}

public record FilterItemCsvMetaViewModel
{
    public Guid Id { get; init; }

    public string GroupLabel { get; init; } = string.Empty;

    public string Label { get; init; } = string.Empty;

    public FilterItemCsvMetaViewModel()
    {
    }

    public FilterItemCsvMetaViewModel(FilterItem filterItem)
    {
        Id = filterItem.Id;
        GroupLabel = filterItem.FilterGroup.Label;
        Label = filterItem.Label;
    }

    public FilterItemCsvMetaViewModel(FilterItemMetaViewModel filterItem, string groupLabel)
    {
        Id = filterItem.Value;
        GroupLabel = groupLabel;
        Label = filterItem.Label;
    }
}

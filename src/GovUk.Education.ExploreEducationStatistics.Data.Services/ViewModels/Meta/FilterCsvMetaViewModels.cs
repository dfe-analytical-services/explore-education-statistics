#nullable enable
using System;
using System.Collections.Generic;
using System.Linq;
using GovUk.Education.ExploreEducationStatistics.Data.Model;

namespace GovUk.Education.ExploreEducationStatistics.Data.Services.ViewModels.Meta;

public record FilterCsvMetaViewModel
{
    public Guid Id { get; init; }

    public string Name { get; init; } = string.Empty;

    public IReadOnlyDictionary<Guid, FilterItemCsvMetaViewModel> Items { get; init; } =
        new Dictionary<Guid, FilterItemCsvMetaViewModel>();

    public FilterCsvMetaViewModel()
    {
    }

    public FilterCsvMetaViewModel(Filter filter)
    {
        Id = filter.Id;
        Name = filter.Name;
        Items = filter.FilterGroups
            .SelectMany(filterGroup => filterGroup.FilterItems)
            .Select(filterItem => new FilterItemCsvMetaViewModel(filterItem))
            .ToDictionary(filterItem => filterItem.Id);
    }

    public FilterCsvMetaViewModel(FilterMetaViewModel filter)
    {
        Id = filter.Id;
        Name = filter.Name;
        Items = filter.Options
            .SelectMany(filterGroup => filterGroup.Value.Options)
            .Select(filterItem => new FilterItemCsvMetaViewModel(filterItem))
            .ToDictionary(filterItem => filterItem.Id);
    }
}

public record FilterItemCsvMetaViewModel
{
    public Guid Id { get; init; }

    public string Label { get; init; } = string.Empty;

    public FilterItemCsvMetaViewModel()
    {
    }

    public FilterItemCsvMetaViewModel(FilterItem filterItem)
    {
        Id = filterItem.Id;
        Label = filterItem.Label;
    }

    public FilterItemCsvMetaViewModel(FilterItemMetaViewModel filterItem)
    {
        Id = filterItem.Value;
        Label = filterItem.Label;
    }
}

using GovUk.Education.ExploreEducationStatistics.Data.Model;

namespace GovUk.Education.ExploreEducationStatistics.Data.ViewModels.Meta;

public record FilterMetaViewModel
{
    public Guid Id { get; init; }
    public string? Hint { get; init; }
    public string Legend { get; init; } = string.Empty;
    public Dictionary<string, FilterGroupMetaViewModel> Options { get; init; } = new();
    public string Name { get; init; } = string.Empty;
    public string? GroupCsvColumn { get; init; }
    public Guid? TotalValue { get; init; }
    public int Order { get; init; }

    public FilterMetaViewModel(Filter filter, int order)
    {
        Id = filter.Id;
        Hint = filter.Hint;
        Legend = filter.Label;
        Name = filter.Name;
        GroupCsvColumn = filter.GroupCsvColumn;
        TotalValue = filter.DefaultFilterItemId;
        Order = order;
    }

    public FilterMetaViewModel()
    {
    }
}

public record FilterGroupMetaViewModel
{
    public Guid Id { get; init; }
    public string Label { get; init; } = string.Empty;
    public List<FilterItemMetaViewModel> Options { get; init; } = new();
    public int Order { get; init; }

    public FilterGroupMetaViewModel(FilterGroup filterGroup, int order)
    {
        Id = filterGroup.Id;
        Label = filterGroup.Label;
        Order = order;
    }

    public FilterGroupMetaViewModel()
    {
    }
}

public record FilterItemMetaViewModel
{
    public string Label { get; init; } = string.Empty;
    public Guid Value { get; init; }

    public FilterItemMetaViewModel(FilterItem filterItem)
    {
        Label = filterItem.Label;
        Value = filterItem.Id;
    }

    public FilterItemMetaViewModel(string label, Guid value)
    {
        Label = label;
        Value = value;
    }

    public FilterItemMetaViewModel()
    {
    }
}

public record FilterUpdateViewModel
{
    public Guid Id { get; init; }

    public List<FilterGroupUpdateViewModel> FilterGroups { get; init; } = new();
}

public record FilterGroupUpdateViewModel
{
    public Guid Id { get; init; }

    public List<Guid> FilterItems { get; init; } = new();
}

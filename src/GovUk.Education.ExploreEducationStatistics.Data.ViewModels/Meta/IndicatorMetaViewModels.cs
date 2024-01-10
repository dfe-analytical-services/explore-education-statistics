using GovUk.Education.ExploreEducationStatistics.Common.Converters;
using GovUk.Education.ExploreEducationStatistics.Common.Model;
using GovUk.Education.ExploreEducationStatistics.Data.Model;
using Newtonsoft.Json;

namespace GovUk.Education.ExploreEducationStatistics.Data.ViewModels.Meta;

public record IndicatorGroupMetaViewModel
{
    public Guid Id { get; init; }
    public string Label { get; init; } = string.Empty;
    public List<IndicatorMetaViewModel> Options { get; init; } = new();
    public int Order { get; init; }

    public IndicatorGroupMetaViewModel(IndicatorGroup indicatorGroup, int order)
    {
        Id = indicatorGroup.Id;
        Label = indicatorGroup.Label;
        Order = order;
    }

    public IndicatorGroupMetaViewModel()
    {
    }
}

public record IndicatorMetaViewModel
{
    public string Label { get; init; } = string.Empty;

    [JsonConverter(typeof(EnumToEnumValueJsonConverter<IndicatorUnit>))]
    public IndicatorUnit Unit { get; init; }

    public Guid Value { get; init; }

    public string Name { get; init; } = string.Empty;

    public int? DecimalPlaces { get; init; }

    public IndicatorMetaViewModel(Indicator indicator)
    {
        DecimalPlaces = indicator.DecimalPlaces;
        Label = indicator.Label;
        Name = indicator.Name;
        Unit = indicator.Unit;
        Value = indicator.Id;
    }

    public IndicatorMetaViewModel()
    {
    }
}

public record IndicatorGroupUpdateViewModel
{
    public Guid Id { get; set; }

    public List<Guid> Indicators { get; set; } = new();
}

#nullable enable
using System;
using GovUk.Education.ExploreEducationStatistics.Common.Converters;
using GovUk.Education.ExploreEducationStatistics.Data.Model;
using Newtonsoft.Json;

namespace GovUk.Education.ExploreEducationStatistics.Data.Services.ViewModels.Meta;

public record IndicatorCsvMetaViewModel
{
    public Guid Id { get; init; }

    public string Label { get; init; } = string.Empty;

    [JsonConverter(typeof(EnumToEnumValueJsonConverter<Unit>))]
    public Unit Unit { get; init; }

    public string Name { get; init; } = string.Empty;

    public int? DecimalPlaces { get; init; }

    public IndicatorCsvMetaViewModel()
    {
    }

    public IndicatorCsvMetaViewModel(Indicator indicator)
    {
        Id = indicator.Id;
        DecimalPlaces = indicator.DecimalPlaces;
        Label = indicator.Label;
        Name = indicator.Name;
        Unit = indicator.Unit;
    }

    public IndicatorCsvMetaViewModel(IndicatorMetaViewModel indicator)
    {
        Id = indicator.Value;
        DecimalPlaces = indicator.DecimalPlaces;
        Label = indicator.Label;
        Name = indicator.Name;
        Unit = indicator.Unit;
    }
}

#nullable enable
using System;
using System.Collections.Generic;
using GovUk.Education.ExploreEducationStatistics.Data.Services.ViewModels.Meta;

namespace GovUk.Education.ExploreEducationStatistics.Data.Api.ViewModels;

public record PermalinkCsvMetaViewModel
{
    public IReadOnlyDictionary<string, FilterCsvMetaViewModel> Filters { get; init; } =
        new Dictionary<string, FilterCsvMetaViewModel>();

    public IReadOnlyDictionary<Guid, Dictionary<string, string>> Locations { get; init; } =
        new Dictionary<Guid, Dictionary<string, string>>();

    public IReadOnlyDictionary<string, IndicatorCsvMetaViewModel> Indicators { get; init; } =
        new Dictionary<string, IndicatorCsvMetaViewModel>();

    public List<string> Headers { get; init; } = new();
}

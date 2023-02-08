#nullable enable
using System;
using System.Collections.Generic;

namespace GovUk.Education.ExploreEducationStatistics.Data.Services.ViewModels.Meta;

public record SubjectCsvMetaViewModel
{
    public IReadOnlyDictionary<string, FilterCsvMetaViewModel> Filters { get; init; } =
        new Dictionary<string, FilterCsvMetaViewModel>();

    public IReadOnlyDictionary<Guid, Dictionary<string, string>> Locations { get; init; } =
        new Dictionary<Guid, Dictionary<string, string>>();

    public IReadOnlyDictionary<string, IndicatorCsvMetaViewModel> Indicators { get; init; } =
        new Dictionary<string, IndicatorCsvMetaViewModel>();

    public List<string> Headers { get; init; } = new();
}

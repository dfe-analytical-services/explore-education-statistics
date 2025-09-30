#nullable enable
using GovUk.Education.ExploreEducationStatistics.Data.ViewModels.Meta;

namespace GovUk.Education.ExploreEducationStatistics.Data.Api.ViewModels;

public record PermalinkCsvMetaViewModel
{
    private Lazy<Dictionary<string, FilterCsvMetaViewModel>> FiltersByGroupingColumnLazy =>
        new(() =>
            Filters
                .Values.Where(filter => filter.GroupCsvColumn != null)
                .ToDictionary(filter => filter.GroupCsvColumn!, filter => filter)
        );

    public IReadOnlyDictionary<string, FilterCsvMetaViewModel> Filters { get; init; } =
        new Dictionary<string, FilterCsvMetaViewModel>();

    public IReadOnlyDictionary<Guid, Dictionary<string, string>> Locations { get; init; } =
        new Dictionary<Guid, Dictionary<string, string>>();

    public IReadOnlyDictionary<string, IndicatorCsvMetaViewModel> Indicators { get; init; } =
        new Dictionary<string, IndicatorCsvMetaViewModel>();

    public List<string> Headers { get; init; } = new();

    public Dictionary<string, FilterCsvMetaViewModel> FiltersByGroupingColumn =>
        FiltersByGroupingColumnLazy.Value;
}

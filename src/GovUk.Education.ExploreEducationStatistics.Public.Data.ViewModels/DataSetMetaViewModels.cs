using System.Text.Json.Serialization;

namespace GovUk.Education.ExploreEducationStatistics.Public.Data.ViewModels;

/// <summary>
/// All the metadata associated with a data set.
/// </summary>
public record DataSetMetaViewModel
{
    /// <summary>
    /// All the filters associated with the data set.
    /// </summary>
    public required IReadOnlyList<FilterOptionsViewModel> Filters { get; init; }

    /// <summary>
    /// All the indicators associated with the data set.
    /// </summary>
    public required IReadOnlyList<IndicatorViewModel> Indicators { get; init; }

    /// <summary>
    /// All the geographic levels associated with the data set.
    /// </summary>
    public required IReadOnlyList<GeographicLevelViewModel> GeographicLevels { get; init; }

    /// <summary>
    /// All the locations associated with the data set, grouped by geographic level.
    /// </summary>
    public required IReadOnlyList<LocationGroupOptionsViewModel> Locations { get; init; }

    /// <summary>
    /// All the time periods associated with the data set.
    /// </summary>
    public required IReadOnlyList<TimePeriodOptionViewModel> TimePeriods { get; init; }
}

/// <summary>
/// The options available for a filterable characteristic about the data set
/// (excluding geography or time).
/// </summary>
public record FilterOptionsViewModel : FilterViewModel
{
    /// <summary>
    /// The filter options belonging to this filter.
    /// </summary>
    [JsonPropertyOrder(1)]
    public required IReadOnlyList<FilterOptionViewModel> Options { get; init; }
}

/// <summary>
/// The options available for a location group in the data set.
/// </summary>
public record LocationGroupOptionsViewModel : LocationGroupViewModel
{
    /// <summary>
    /// The locations belonging to this level.
    /// </summary>
    [JsonPropertyOrder(1)]
    public required IReadOnlyList<LocationOptionViewModel> Options { get; init; }
}

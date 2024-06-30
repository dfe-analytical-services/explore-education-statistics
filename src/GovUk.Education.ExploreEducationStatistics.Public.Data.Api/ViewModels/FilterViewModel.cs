namespace GovUk.Education.ExploreEducationStatistics.Public.Data.Api.ViewModels;

/// <summary>
/// A filterable characteristic (excluding geography or time) of a data set.
/// </summary>
public record FilterViewModel
{
    /// <summary>
    /// The ID of the filter.
    /// </summary>
    public required string Id { get; init; }

    /// <summary>
    /// A hint to assist in describing the filter.
    /// </summary>
    public string Hint { get; init; } = string.Empty;

    /// <summary>
    /// The human-readable label describing the filter.
    /// </summary>
    public required string Label { get; init; }
}

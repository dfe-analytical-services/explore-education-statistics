namespace GovUk.Education.ExploreEducationStatistics.Public.Data.Api.ViewModels;

/// <summary>
/// A filterable option that can be used to filter a data set.
/// </summary>
public record FilterOptionViewModel
{
    /// <summary>
    /// The ID of the filter option.
    /// </summary>
    public required string Id { get; init; }

    /// <summary>
    /// The human-readable label describing the filter option.
    /// </summary>
    public required string Label { get; init; }

    /// <summary>
    /// Whether the filter option is an aggregate (i.e. ‘all’ or a ‘total’) of the other filter options.
    /// </summary>
    public bool? IsAggregate { get; init; }
}

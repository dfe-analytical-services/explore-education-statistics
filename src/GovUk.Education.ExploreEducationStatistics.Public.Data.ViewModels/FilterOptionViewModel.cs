namespace GovUk.Education.ExploreEducationStatistics.Public.Data.ViewModels;

/// <summary>
/// A filterable option that can be used to filter a data set.
/// </summary>
public record FilterOptionViewModel
{
    /// <summary>
    /// The ID of the filter option.
    /// </summary>
    /// <example>q1g3J</example>
    public required string Id { get; init; }

    /// <summary>
    /// The human-readable label describing the filter option.
    /// </summary>
    /// <example>State-funded primary</example>
    public required string Label { get; init; }
}

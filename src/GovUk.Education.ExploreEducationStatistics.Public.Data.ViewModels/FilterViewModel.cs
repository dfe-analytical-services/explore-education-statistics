namespace GovUk.Education.ExploreEducationStatistics.Public.Data.ViewModels;

/// <summary>
/// A filterable characteristic (excluding geography or time) of a data set.
/// </summary>
public record FilterViewModel
{
    /// <summary>
    /// The ID of the filter.
    /// </summary>
    /// <example>BRlj4</example>
    public required string Id { get; init; }

    /// <summary>
    /// The name of the filter CSV column.
    /// </summary>
    /// <example>school_type</example>
    public required string Column { get; init; }

    /// <summary>
    /// The human-readable label describing the filter.
    /// </summary>
    /// <example>School type</example>
    public required string Label { get; init; }

    /// <summary>
    /// A hint to assist in describing the filter.
    /// </summary>
    /// <example>Additional detail about the filter.</example>
    public string Hint { get; init; } = string.Empty;
}

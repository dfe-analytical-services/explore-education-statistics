using GovUk.Education.ExploreEducationStatistics.Public.Data.Model;

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
    /// The name of the filter CSV column.
    /// </summary>
    /// <example>school_type</example>
    public required string Column { get; init; }

    /// <summary>
    /// A hint to assist in describing the filter.
    /// </summary>
    public string Hint { get; init; } = string.Empty;

    /// <summary>
    /// The human-readable label describing the filter.
    /// </summary>
    public required string Label { get; init; }

    public static FilterViewModel Create(FilterMeta meta)
    {
        return new FilterViewModel
        {
            Id = meta.PublicId,
            Column = meta.Column,
            Label = meta.Label,
            Hint = meta.Hint
        };
    }
}

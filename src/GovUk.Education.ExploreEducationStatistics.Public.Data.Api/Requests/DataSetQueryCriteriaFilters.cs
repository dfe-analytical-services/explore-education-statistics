namespace GovUk.Education.ExploreEducationStatistics.Public.Data.Api.Requests;

/// <summary>
/// The filter option criteria to filter results by in a data set query.
/// </summary>
public record DataSetQueryCriteriaFilters
{
    /// <summary>
    /// Filter the results to have a filter option matching this ID.
    /// </summary>
    public string? Eq { get; init; }

    /// <summary>
    /// Filter the results to not have a filter option matching this ID.
    /// </summary>
    public string? NotEq { get; init; }

    /// <summary>
    /// Filter the results to have a filter option matching at least one of these IDs.
    /// </summary>
    public IReadOnlyList<string>? In { get; init; }

    /// <summary>
    /// Filter the results to not have a filter option matching any of these IDs.
    /// </summary>
    public IReadOnlyList<string>? NotIn { get; init; }
}

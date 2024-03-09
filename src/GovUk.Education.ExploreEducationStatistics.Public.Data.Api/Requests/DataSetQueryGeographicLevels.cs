using GovUk.Education.ExploreEducationStatistics.Common.Model.Data;

namespace GovUk.Education.ExploreEducationStatistics.Public.Data.Api.Requests;

/// <summary>
/// The geographic levels to filter results by in a data set query.
/// </summary>
public record DataSetQueryGeographicLevels
{
    /// <summary>
    /// Filter the results to be in this geographic level.
    /// </summary>
    public GeographicLevel? Eq { get; init; }

    /// <summary>
    /// Filter the results to not be in this geographic level.
    /// </summary>
    public GeographicLevel? NotEq { get; init; }

    /// <summary>
    /// Filter the results to be in one of these geographic levels.
    /// </summary>
    public IReadOnlyList<GeographicLevel>? In { get; init; }

    /// <summary>
    /// Filter the results to not be in one of these geographic levels.
    /// </summary>
    public IReadOnlyList<GeographicLevel>? NotIn { get; init; }
}

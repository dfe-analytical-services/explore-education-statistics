namespace GovUk.Education.ExploreEducationStatistics.Public.Data.Api.Requests;

/// <summary>
/// The location option criteria to filter results by in a data set query.
///
/// The results can be matched by either the location option's ID or a code.
/// Note the following differences:
///
/// - IDs only match a **single location**
/// - Codes may match **multiple locations**
///
/// Whilst codes are generally unique to a single location, they can be
/// used for multiple locations. This may match more results than you
/// expect so it's recommended to use IDs where possible.
/// </summary>
public record DataSetQueryCriteriaLocations
{
    /// <summary>
    /// Filter the results to be in this location.
    /// </summary>
    public DataSetQueryLocation? Eq { get; init; }

    /// <summary>
    /// Filter the results to not be in this location.
    /// </summary>
    public DataSetQueryLocation? NotEq { get; init; }

    /// <summary>
    /// Filter the results to be in one of these locations.
    /// </summary>
    public IReadOnlyList<DataSetQueryLocation>? In { get; init; }

    /// <summary>
    /// Filter the results to not be in one of these locations.
    /// </summary>
    public IReadOnlyList<DataSetQueryLocation>? NotIn { get; init; }
}

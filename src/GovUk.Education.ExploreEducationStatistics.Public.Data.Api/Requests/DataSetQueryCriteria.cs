namespace GovUk.Education.ExploreEducationStatistics.Public.Data.Api.Requests;

/// <summary>
/// The criteria to filter results by in a data set query.
///
/// All parts of the criteria must resolve to true to match a result.
/// </summary>
public record DataSetQueryCriteria
{
    /// <summary>
    /// Query criteria relating to filter options.
    /// </summary>
    public DataSetQueryFilters? Filters { get; init; }

    /// <summary>
    /// Query criteria relating to geographic levels.
    /// </summary>
    public DataSetQueryGeographicLevels? GeographicLevels { get; init; }

    /// <summary>
    /// Query criteria relating to location options.
    /// </summary>
    public DataSetQueryLocations? Locations { get; init; }
 
    /// <summary>
    /// Query criteria relating to time periods.
    /// </summary>
    public DataSetQueryTimePeriods? TimePeriods { get; init; }

    /// <summary>
    /// The indicators to return values for.
    /// </summary>
    public required IReadOnlyList<string> Indicators { get; init; }
}

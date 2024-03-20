namespace GovUk.Education.ExploreEducationStatistics.Public.Data.Api.Requests;

/// <summary>
/// Criteria outlining the facets of the data to filter results by in the data set query.
///
/// All parts of the criteria must resolve to true to match a result.
/// </summary>
public record DataSetQueryCriteriaFacets : DataSetQueryCriteria
{
    /// <summary>
    /// Query criteria relating to filter options.
    /// </summary>
    public DataSetQueryCriteriaFilters? Filters { get; init; }

    /// <summary>
    /// Query criteria relating to geographic levels.
    /// </summary>
    public DataSetQueryCriteriaGeographicLevels? GeographicLevels { get; init; }

    /// <summary>
    /// Query criteria relating to location options.
    /// </summary>
    public DataSetQueryCriteriaLocations? Locations { get; init; }
 
    /// <summary>
    /// Query criteria relating to time periods.
    /// </summary>
    public DataSetQueryCriteriaTimePeriods? TimePeriods { get; init; }
}

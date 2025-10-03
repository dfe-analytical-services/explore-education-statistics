using FluentValidation;

namespace GovUk.Education.ExploreEducationStatistics.Public.Data.Api.Requests;

/// <summary>
/// A set of criteria specifying which facets the query should match results with.
///
/// All parts of the criteria must resolve to true to match a result.
/// </summary>
public record DataSetQueryCriteriaFacets : IDataSetQueryCriteria
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

    public class Validator : AbstractValidator<DataSetQueryCriteriaFacets>
    {
        public Validator()
        {
            RuleFor(q => q.Filters)
                .SetValidator(new DataSetQueryCriteriaFilters.Validator()!)
                .When(q => q.Filters is not null);

            RuleFor(q => q.GeographicLevels)
                .SetValidator(new DataSetQueryCriteriaGeographicLevels.Validator()!)
                .When(q => q.GeographicLevels is not null);

            RuleFor(q => q.Locations)
                .SetValidator(new DataSetQueryCriteriaLocations.Validator()!)
                .When(q => q.Locations is not null);

            RuleFor(q => q.TimePeriods)
                .SetValidator(new DataSetQueryCriteriaTimePeriods.Validator()!)
                .When(q => q.TimePeriods is not null);
        }
    }
}

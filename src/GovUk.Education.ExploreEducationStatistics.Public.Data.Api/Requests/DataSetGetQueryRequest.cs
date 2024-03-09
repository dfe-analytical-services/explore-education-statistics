using FluentValidation;

namespace GovUk.Education.ExploreEducationStatistics.Public.Data.Api.Requests;

public record DataSetGetQueryRequest
{
    /// <summary>
    /// Query criteria relating to filter options.
    /// </summary>
    public DataSetGetQueryFilters? Filters { get; init; }

    /// <summary>
    /// Query criteria relating to geographic levels.
    /// </summary>
    public DataSetGetQueryGeographicLevels? GeographicLevels { get; init; }

    /// <summary>
    /// Query criteria relating to location options.
    /// </summary>
    public DataSetGetQueryLocations? Locations { get; init; }

    /// <summary>
    /// Query criteria relating to time periods.
    /// </summary>
    public DataSetGetQueryTimePeriods? TimePeriods { get; init; }

    /// <summary>
    /// The indicators to return values for.
    /// </summary>
    public required IReadOnlyList<string> Indicators { get; init; }

    public DataSetQueryCriteria ToCriteria()
    {
        return new DataSetQueryCriteria
        {
        };
    }

    public class Validator : AbstractValidator<DataSetGetQueryRequest>
    {
        public Validator()
        {
            RuleFor(q => q.Filters)
                .SetValidator(new DataSetGetQueryFilters.Validator()!)
                .When(q => q.Filters is not null);

            RuleFor(q => q.GeographicLevels)
                .SetValidator(new DataSetGetQueryGeographicLevels.Validator()!)
                .When(q => q.GeographicLevels is not null);

            RuleFor(q => q.Locations)
                .SetValidator(new DataSetGetQueryLocations.Validator()!)
                .When(q => q.Locations is not null);

            RuleFor(q => q.TimePeriods)
                .SetValidator(new DataSetGetQueryTimePeriods.Validator()!)
                .When(q => q.TimePeriods is not null);

            RuleFor(q => q.Indicators)
                .NotEmpty();
            RuleForEach(q => q.Indicators)
                .NotEmpty()
                .MaximumLength(100);
        }
    }
}

using System.ComponentModel;
using FluentValidation;
using GovUk.Education.ExploreEducationStatistics.Common.ModelBinding;
using GovUk.Education.ExploreEducationStatistics.Public.Data.Api.Validators;
using Microsoft.AspNetCore.Mvc;

namespace GovUk.Education.ExploreEducationStatistics.Public.Data.Api.Requests;

/// <summary>
/// A data set GET query request.
/// </summary>
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
    /// The IDs of indicators to return values for.
    /// </summary>
    [FromQuery, QuerySeparator]
    public required IReadOnlyList<string>? Indicators { get; init; }

    /// <summary>
    /// The sorts to apply to the results. Sorts at the start of the
    /// list will be applied first.
    ///
    /// By default, results are sorted by time period in descending order.
    /// </summary>
    [FromQuery, QuerySeparator]
    public IReadOnlyList<string>? Sorts { get; init; }

    /// <summary>
    /// Enable debug mode. Results will be formatted with human-readable
    /// labels to assist in identification.
    ///
    /// This **should not** be enabled in a production environment.
    /// </summary>
    // ReSharper disable once UnusedAutoPropertyAccessor.Global
    public bool Debug { get; init; }

    /// <summary>
    /// The page of results to fetch.
    /// </summary>
    [DefaultValue(1)]
    public int Page { get; init; } = 1;

    /// <summary>
    /// The maximum number of results per page.
    /// </summary>
    [DefaultValue(1000)]
    public int PageSize { get; init; } = 1000;

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

            RuleForEach(q => q.Indicators)
                .NotEmpty()
                .MaximumLength(40);

            When(q => q.Sorts is not null, () =>
            {
                RuleFor(q => q.Sorts)
                    .NotEmpty();
                RuleForEach(q => q.Sorts)
                    .SortString();
            });

            RuleFor(request => request.Page)
                .GreaterThanOrEqualTo(1);
            
            RuleFor(request => request.PageSize)
                .InclusiveBetween(1, 10000);
        }
    }
}

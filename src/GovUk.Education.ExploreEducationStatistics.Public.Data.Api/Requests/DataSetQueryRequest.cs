using System.ComponentModel;
using FluentValidation;

namespace GovUk.Education.ExploreEducationStatistics.Public.Data.Api.Requests;

/// <summary>
/// A data set query to match results against.
/// </summary>
public record DataSetQueryRequest
{
    /// <summary>
    /// The criteria to match.
    /// </summary>
    public IDataSetQueryCriteria? Criteria { get; init; }

    /// <summary>
    /// The IDs of indicators to return values for.
    /// Omitting this parameter will select all indicators.
    /// </summary>
    /// <example>["C2ySJ", "q4X3J"]</example>
    public IReadOnlyList<string>? Indicators { get; init; }

    /// <summary>
    /// The sorts to sort the results by. Sorts at the start of the
    /// list will be applied first.
    ///
    /// By default, results are sorted by time period in descending order.
    /// </summary>
    public IReadOnlyList<DataSetQuerySort>? Sorts { get; init; }

    /// <summary>
    /// Enable debug mode. Results will be formatted with human-readable
    /// labels to assist in identification.
    ///
    /// This **should not** be enabled in a production environment.
    /// </summary>
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

    public class Validator : AbstractValidator<DataSetQueryRequest>
    {
        public Validator()
        {
            RuleForEach(q => q.Indicators)
                .NotEmpty()
                .MaximumLength(40);

            RuleFor(q => q.Criteria)
                .SetInheritanceValidator(v =>
                {
                    v.Add(new DataSetQueryCriteriaAnd.Validator());
                    v.Add(new DataSetQueryCriteriaOr.Validator());
                    v.Add(new DataSetQueryCriteriaNot.Validator());
                    v.Add(new DataSetQueryCriteriaFacets.Validator());
                });

            When(q => q.Sorts is not null, () =>
            {
                RuleFor(q => q.Sorts)
                    .NotEmpty();
                RuleForEach(q => q.Sorts)
                    .NotNull()
                    .SetValidator(new DataSetQuerySort.Validator());
            });

            RuleFor(request => request.Page)
                .GreaterThanOrEqualTo(1);
            
            RuleFor(request => request.PageSize)
                .InclusiveBetween(1, 10000);
        }
    }
}

using FluentValidation;
using GovUk.Education.ExploreEducationStatistics.Common.ModelBinding;
using Microsoft.AspNetCore.Mvc;

namespace GovUk.Education.ExploreEducationStatistics.Public.Data.Api.Requests;

public record DataSetGetQueryFilters
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
    [FromQuery, QuerySeparator]
    public IReadOnlyList<string>? In { get; init; }

    /// <summary>
    /// Filter the results to not have a filter option matching any of these IDs.
    /// </summary>
    [FromQuery, QuerySeparator]
    public IReadOnlyList<string>? NotIn { get; init; }

    public DataSetQueryCriteriaFilters ToCriteria()
    {
        return new DataSetQueryCriteriaFilters
        {
            Eq = Eq,
            NotEq = NotEq,
            In = In,
            NotIn = NotIn
        };
    }

    public class Validator : AbstractValidator<DataSetGetQueryFilters>
    {
        public Validator()
        {
            RuleFor(q => q.Eq)
                .NotEmpty()
                .MaximumLength(10)
                .When(q => q.Eq is not null);

            RuleFor(q => q.NotEq)
                .NotEmpty()
                .MaximumLength(10)
                .When(q => q.NotEq is not null);

            When(q => q.In is not null, () =>
            {
                RuleFor(q => q.In)
                    .NotEmpty();
                
                RuleForEach(q => q.In)
                    .NotEmpty()
                    .MaximumLength(10);
            });

            When(q => q.NotIn is not null, () =>
            {
                RuleFor(q => q.NotIn)
                    .NotEmpty();

                RuleForEach(q => q.NotIn)
                    .NotEmpty()
                    .MaximumLength(10);
            });
        }
    }
}

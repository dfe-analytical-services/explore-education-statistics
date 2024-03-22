using FluentValidation;
using GovUk.Education.ExploreEducationStatistics.Common.ModelBinding;
using GovUk.Education.ExploreEducationStatistics.Public.Data.Api.Validators;
using Microsoft.AspNetCore.Mvc;

namespace GovUk.Education.ExploreEducationStatistics.Public.Data.Api.Requests;

public record DataSetGetQueryLocations
{
    /// <summary>
    /// Filter the results to be in this location.
    /// </summary>
    public string? Eq { get; init; }

    /// <summary>
    /// Filter the results to not be in this location.
    /// </summary>
    public string? NotEq { get; init; }

    /// <summary>
    /// Filter the results to be in one of these locations.
    /// </summary>
    [FromQuery, QuerySeparator]
    public IReadOnlyList<string>? In { get; init; }

    /// <summary>
    /// Filter the results not to be in one of these locations.
    /// </summary>
    [FromQuery, QuerySeparator]
    public IReadOnlyList<string>? NotIn { get; init; }

    public DataSetQueryCriteriaLocations ToCriteria()
    {
        return new DataSetQueryCriteriaLocations
        {
            Eq = Eq is not null ? DataSetQueryLocation.Parse(Eq) : null,
            NotEq = NotEq is not null ? DataSetQueryLocation.Parse(NotEq) : null,
            In = In?.Select(DataSetQueryLocation.Parse).ToList(),
            NotIn = NotIn?.Select(DataSetQueryLocation.Parse).ToList()
        };
    }

    public class Validator : AbstractValidator<DataSetGetQueryLocations>
    {
        public Validator()
        {
            RuleFor(request => request.Eq)!
                .LocationString()
                .When(request => request.Eq is not null);

            RuleFor(request => request.NotEq)!
                .LocationString()
                .When(request => request.NotEq is not null);

            When(q => q.In is not null, () =>
            {
                RuleFor(request => request.In)
                    .NotEmpty();
                RuleForEach(request => request.In)
                    .LocationString();
            });

            When(q => q.NotIn is not null, () =>
            {
                RuleFor(request => request.NotIn)
                    .NotEmpty();
                RuleForEach(request => request.NotIn)
                    .LocationString();
            });
        }
    }
}

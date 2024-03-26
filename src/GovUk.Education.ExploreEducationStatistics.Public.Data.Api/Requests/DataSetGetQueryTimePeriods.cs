using FluentValidation;
using GovUk.Education.ExploreEducationStatistics.Common.ModelBinding;
using GovUk.Education.ExploreEducationStatistics.Public.Data.Api.Validators;
using Microsoft.AspNetCore.Mvc;

namespace GovUk.Education.ExploreEducationStatistics.Public.Data.Api.Requests;

public record DataSetGetQueryTimePeriods
{
    /// <summary>
    /// Filter the results to be in this time period.
    /// </summary>
    public string? Eq { get; init; }

    /// <summary>
    /// Filter the results to not be in this time period.
    /// </summary>
    public string? NotEq { get; init; }

    /// <summary>
    /// Filter the results to be in one of these time periods.
    /// </summary>
    [FromQuery, QuerySeparator]
    public IReadOnlyList<string>? In { get; init; }

    /// <summary>
    /// Filter the results to not be in one of these time periods.
    /// </summary>
    [FromQuery, QuerySeparator]
    public IReadOnlyList<string>? NotIn { get; init; }

    /// <summary>
    /// Filter the results to be in a time period that is
    /// chronologically greater than the one specified.
    /// </summary>
    public string? Gt { get; init; }

    /// <summary>
    /// Filter the results to be in a time period that is
    /// chronologically greater than or equal to the one specified.
    /// </summary>
    public string? Gte { get; init; }

    /// <summary>
    /// Filter the results to be in a time period that is
    /// chronologically less than the one specified.
    /// </summary>
    public string? Lt { get; init; }

    /// <summary>
    /// Filter the results to be in a time period that is
    /// chronologically less than or equal to the one specified.
    /// </summary>
    public string? Lte { get; init; }

    public DataSetQueryCriteriaTimePeriods ToCriteria()
    {
        return new DataSetQueryCriteriaTimePeriods
        {
            Eq = Eq is not null ? DataSetQueryTimePeriod.FromString(Eq) : null,
            NotEq = NotEq is not null ? DataSetQueryTimePeriod.FromString(NotEq) : null,
            In = In?.Select(DataSetQueryTimePeriod.FromString).ToList(),
            NotIn = NotIn?.Select(DataSetQueryTimePeriod.FromString).ToList(),
            Gt = Gt is not null ? DataSetQueryTimePeriod.FromString(Gt) : null,
            Gte = Gte is not null ? DataSetQueryTimePeriod.FromString(Gte) : null,
            Lt = Lt is not null ? DataSetQueryTimePeriod.FromString(Lt) : null,
            Lte = Lte is not null ? DataSetQueryTimePeriod.FromString(Lte) : null
        };
    }

    public class Validator : AbstractValidator<DataSetGetQueryTimePeriods>
    {
        public Validator()
        {
            RuleFor(request => request.Eq)!
                .TimePeriodString()
                .When(request => request.Eq is not null);

            RuleFor(request => request.NotEq)!
                .TimePeriodString()
                .When(request => request.NotEq is not null);

            When(q => q.In is not null, () =>
            {
                RuleFor(request => request.In)
                    .NotEmpty();
                RuleForEach(request => request.In)
                    .TimePeriodString();
            });

            When(q => q.NotIn is not null, () =>
            {
                RuleFor(request => request.NotIn)
                    .NotEmpty();
                RuleForEach(request => request.NotIn)
                    .TimePeriodString();
            });

            RuleFor(request => request.Gt)!
                .TimePeriodString()
                .When(request => request.Gt is not null);

            RuleFor(request => request.Gte)!
                .TimePeriodString()
                .When(request => request.Gte is not null);

            RuleFor(request => request.Lt)!
                .TimePeriodString()
                .When(request => request.Lt is not null);

            RuleFor(request => request.Lte)!
                .TimePeriodString()
                .When(request => request.Lte is not null);
        }
    }
}

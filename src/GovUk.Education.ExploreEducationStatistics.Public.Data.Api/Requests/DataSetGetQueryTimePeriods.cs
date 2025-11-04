using FluentValidation;
using GovUk.Education.ExploreEducationStatistics.Common.ModelBinding;
using GovUk.Education.ExploreEducationStatistics.Public.Data.Api.Validators;
using Microsoft.AspNetCore.Mvc;

namespace GovUk.Education.ExploreEducationStatistics.Public.Data.Api.Requests;

/// <summary>
/// The time period criteria to filter results by in a data set GET query.
/// </summary>
public record DataSetGetQueryTimePeriods
{
    /// <summary>
    /// Filter the results to be in this time period.
    /// </summary>
    /// <example>2024|CY</example>
    public string? Eq { get; init; }

    /// <summary>
    /// Filter the results to not be in this time period.
    /// </summary>
    /// <example>2024/2025|AY</example>
    public string? NotEq { get; init; }

    /// <summary>
    /// Filter the results to be in one of these time periods.
    /// </summary>
    /// <example>["2022|CY", "2023|CY", "2024|CY"]</example>
    [FromQuery, QuerySeparator]
    public IReadOnlyList<string>? In { get; init; }

    /// <summary>
    /// Filter the results to not be in one of these time periods.
    /// </summary>
    /// <example>["2020|M1", "2020|M2", "2020|M3"]</example>
    [FromQuery, QuerySeparator]
    public IReadOnlyList<string>? NotIn { get; init; }

    /// <summary>
    /// Filter the results to be in a time period that is
    /// chronologically greater than the one specified.
    /// </summary>
    /// <example>2017/2018|AY</example>
    public string? Gt { get; init; }

    /// <summary>
    /// Filter the results to be in a time period that is
    /// chronologically greater than or equal to the one specified.
    /// </summary>
    /// <example>2017|CY</example>
    public string? Gte { get; init; }

    /// <summary>
    /// Filter the results to be in a time period that is
    /// chronologically less than the one specified.
    /// </summary>
    /// <example>2023/2024|AY</example>
    public string? Lt { get; init; }

    /// <summary>
    /// Filter the results to be in a time period that is
    /// chronologically less than or equal to the one specified.
    /// </summary>
    /// <example>2023|CY</example>
    public string? Lte { get; init; }

    public DataSetQueryCriteriaTimePeriods ToCriteria()
    {
        return new DataSetQueryCriteriaTimePeriods
        {
            Eq = Eq is not null ? DataSetQueryTimePeriod.Parse(Eq) : null,
            NotEq = NotEq is not null ? DataSetQueryTimePeriod.Parse(NotEq) : null,
            In = In?.Select(DataSetQueryTimePeriod.Parse).ToList(),
            NotIn = NotIn?.Select(DataSetQueryTimePeriod.Parse).ToList(),
            Gt = Gt is not null ? DataSetQueryTimePeriod.Parse(Gt) : null,
            Gte = Gte is not null ? DataSetQueryTimePeriod.Parse(Gte) : null,
            Lt = Lt is not null ? DataSetQueryTimePeriod.Parse(Lt) : null,
            Lte = Lte is not null ? DataSetQueryTimePeriod.Parse(Lte) : null,
        };
    }

    public class Validator : AbstractValidator<DataSetGetQueryTimePeriods>
    {
        public Validator()
        {
            RuleFor(request => request.Eq)!.TimePeriodString().When(request => request.Eq is not null);

            RuleFor(request => request.NotEq)!.TimePeriodString().When(request => request.NotEq is not null);

            When(
                q => q.In is not null,
                () =>
                {
                    RuleFor(request => request.In).NotEmpty();
                    RuleForEach(request => request.In).TimePeriodString();
                }
            );

            When(
                q => q.NotIn is not null,
                () =>
                {
                    RuleFor(request => request.NotIn).NotEmpty();
                    RuleForEach(request => request.NotIn).TimePeriodString();
                }
            );

            RuleFor(request => request.Gt)!.TimePeriodString().When(request => request.Gt is not null);

            RuleFor(request => request.Gte)!.TimePeriodString().When(request => request.Gte is not null);

            RuleFor(request => request.Lt)!.TimePeriodString().When(request => request.Lt is not null);

            RuleFor(request => request.Lte)!.TimePeriodString().When(request => request.Lte is not null);
        }
    }
}

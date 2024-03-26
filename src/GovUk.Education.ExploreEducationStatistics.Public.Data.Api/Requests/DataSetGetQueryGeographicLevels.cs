using FluentValidation;
using GovUk.Education.ExploreEducationStatistics.Common.Model.Data;
using GovUk.Education.ExploreEducationStatistics.Common.ModelBinding;
using GovUk.Education.ExploreEducationStatistics.Common.Utils;
using GovUk.Education.ExploreEducationStatistics.Common.Validators;
using GovUk.Education.ExploreEducationStatistics.Public.Data.Api.Swagger;
using Microsoft.AspNetCore.Mvc;

namespace GovUk.Education.ExploreEducationStatistics.Public.Data.Api.Requests;

public record DataSetGetQueryGeographicLevels
{
    /// <summary>
    /// Filter the results to be in this geographic level.
    /// </summary>
    [SwaggerEnum(typeof(GeographicLevel))]
    public string? Eq { get; init; }

    /// <summary>
    /// Filter the results to not be in this geographic level.
    /// </summary>
    [SwaggerEnum(typeof(GeographicLevel))]
    public string? NotEq { get; init; }

    /// <summary>
    /// Filter the results to be in one of these geographic levels.
    /// </summary>
    [FromQuery]
    [QuerySeparator]
    [SwaggerEnum(typeof(GeographicLevel))]
    public IReadOnlyList<string>? In { get; init; }

    /// <summary>
    /// Filter the results to not be in one of these geographic levels.
    /// </summary>
    [FromQuery]
    [QuerySeparator]
    [SwaggerEnum(typeof(GeographicLevel))]
    public IReadOnlyList<string>? NotIn { get; init; }

    public DataSetQueryCriteriaGeographicLevels ToCriteria()
    {
        return new DataSetQueryCriteriaGeographicLevels
        {
            Eq = Eq,
            NotEq = NotEq,
            In = In,
            NotIn = NotIn
        };
    }

    public class Validator : AbstractValidator<DataSetGetQueryGeographicLevels>
    {
        public Validator()
        {
            RuleFor(q => q.Eq)
                .AllowedValue(EnumUtil.GetEnumValues<GeographicLevel>())
                .When(q => q.Eq is not null);

            RuleFor(q => q.NotEq)
                .AllowedValue(EnumUtil.GetEnumValues<GeographicLevel>())
                .When(q => q.NotEq is not null);

            When(q => q.In is not null, () =>
            {
                RuleFor(q => q.In)
                    .NotEmpty();
                RuleForEach(q => q.In)
                    .AllowedValue(EnumUtil.GetEnumValues<GeographicLevel>());
            });

            When(q => q.NotIn is not null, () =>
            {
                RuleFor(q => q.NotIn)
                    .NotEmpty();
                RuleForEach(q => q.NotIn)
                    .AllowedValue(EnumUtil.GetEnumValues<GeographicLevel>());
            });
        }
    }
}

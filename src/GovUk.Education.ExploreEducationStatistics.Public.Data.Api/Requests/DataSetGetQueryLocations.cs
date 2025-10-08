using FluentValidation;
using GovUk.Education.ExploreEducationStatistics.Common.ModelBinding;
using GovUk.Education.ExploreEducationStatistics.Public.Data.Api.Validators;
using Microsoft.AspNetCore.Mvc;

namespace GovUk.Education.ExploreEducationStatistics.Public.Data.Api.Requests;

/// <summary>
/// The location option criteria to filter results by in a data set GET query.
///
/// The results can be matched by either the location option's ID or a code.
/// Note the following differences:
///
/// - IDs only match a **single location**
/// - Codes may match **multiple locations**
///
/// Whilst codes are generally unique to a single location, they can be
/// used for multiple locations. This may match more results than you
/// expect so it's recommended to use IDs where possible.
/// </summary>
public record DataSetGetQueryLocations
{
    /// <summary>
    /// Filter the results to be in this location.
    /// </summary>
    /// <example>NAT|id|3dCWP</example>
    public string? Eq { get; init; }

    /// <summary>
    /// Filter the results to not be in this location.
    /// </summary>
    /// <example>REG|code|E12000003</example>
    public string? NotEq { get; init; }

    /// <summary>
    /// Filter the results to be in one of these locations.
    /// </summary>
    /// <example>["LA|code|E08000003", "LA|oldCode|373"]</example>
    [FromQuery, QuerySeparator]
    public IReadOnlyList<string>? In { get; init; }

    /// <summary>
    /// Filter the results not to be in one of these locations.
    /// </summary>
    /// <example>["SCH|urn|123456", "SCH|laEstab|1234567"]</example>
    [FromQuery, QuerySeparator]
    public IReadOnlyList<string>? NotIn { get; init; }

    public DataSetQueryCriteriaLocations ToCriteria()
    {
        return new DataSetQueryCriteriaLocations
        {
            Eq = Eq is not null ? IDataSetQueryLocation.Parse(Eq) : null,
            NotEq = NotEq is not null ? IDataSetQueryLocation.Parse(NotEq) : null,
            In = In?.Select(IDataSetQueryLocation.Parse).ToList(),
            NotIn = NotIn?.Select(IDataSetQueryLocation.Parse).ToList(),
        };
    }

    public class Validator : AbstractValidator<DataSetGetQueryLocations>
    {
        public Validator()
        {
            RuleFor(request => request.Eq)!.LocationString().When(request => request.Eq is not null);

            RuleFor(request => request.NotEq)!.LocationString().When(request => request.NotEq is not null);

            When(
                q => q.In is not null,
                () =>
                {
                    RuleFor(request => request.In).NotEmpty();
                    RuleForEach(request => request.In).LocationString();
                }
            );

            When(
                q => q.NotIn is not null,
                () =>
                {
                    RuleFor(request => request.NotIn).NotEmpty();
                    RuleForEach(request => request.NotIn).LocationString();
                }
            );
        }
    }
}

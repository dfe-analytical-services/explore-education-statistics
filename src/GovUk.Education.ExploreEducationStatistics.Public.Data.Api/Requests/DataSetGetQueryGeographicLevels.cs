using GovUk.Education.ExploreEducationStatistics.Common.Model.Data;
using GovUk.Education.ExploreEducationStatistics.Common.ModelBinding;
using GovUk.Education.ExploreEducationStatistics.Public.Data.Api.Swagger;
using Microsoft.AspNetCore.Mvc;

namespace GovUk.Education.ExploreEducationStatistics.Public.Data.Api.Requests;

/// <summary>
/// The geographic levels criteria to filter results by in a data set GET query.
/// </summary>
public record DataSetGetQueryGeographicLevels : DataSetQueryCriteriaGeographicLevels
{
    /// <summary>
    /// Filter the results to be in one of these geographic levels.
    /// </summary>
    /// <example>["NAT", "LA"]</example>
    [FromQuery]
    [QuerySeparator]
    [SwaggerEnum(typeof(GeographicLevel))]
    public override IReadOnlyList<string>? In { get; init; }

    /// <summary>
    /// Filter the results to not be in one of these geographic levels.
    /// </summary>
    /// <example>["REG", "LAD"]</example>
    [FromQuery]
    [QuerySeparator]
    [SwaggerEnum(typeof(GeographicLevel))]
    public override IReadOnlyList<string>? NotIn { get; init; }

    public DataSetQueryCriteriaGeographicLevels ToCriteria()
    {
        return new DataSetQueryCriteriaGeographicLevels
        {
            Eq = Eq,
            NotEq = NotEq,
            In = In,
            NotIn = NotIn,
        };
    }

    public new class Validator : DataSetQueryCriteriaGeographicLevels.Validator;
}

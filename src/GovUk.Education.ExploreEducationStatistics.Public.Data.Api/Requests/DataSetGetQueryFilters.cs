using GovUk.Education.ExploreEducationStatistics.Common.ModelBinding;
using Microsoft.AspNetCore.Mvc;

namespace GovUk.Education.ExploreEducationStatistics.Public.Data.Api.Requests;

public record DataSetGetQueryFilters : DataSetQueryCriteriaFilters
{
    /// <summary>
    /// Filter the results to have a filter option matching at least one of these IDs.
    /// </summary>
    [FromQuery, QuerySeparator]
    public override IReadOnlyList<string>? In { get; init; }

    /// <summary>
    /// Filter the results to not have a filter option matching any of these IDs.
    /// </summary>
    [FromQuery, QuerySeparator]
    public override IReadOnlyList<string>? NotIn { get; init; }

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

    public new class Validator : DataSetQueryCriteriaFilters.Validator;
}

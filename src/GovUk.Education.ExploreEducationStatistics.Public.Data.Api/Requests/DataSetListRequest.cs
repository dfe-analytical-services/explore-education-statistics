using System.ComponentModel;
using FluentValidation;

namespace GovUk.Education.ExploreEducationStatistics.Public.Data.Api.Requests;

public record DataSetListRequest
{
    /// <summary>
    /// The page of results to fetch.
    /// </summary>
    [DefaultValue(1)]
    public int Page { get; init; } = 1;

    /// <summary>
    /// The maximum number of results per page.
    /// </summary>
    [DefaultValue(10)]
    public int PageSize { get; init; } = 10;

    public class Validator : AbstractValidator<DataSetListRequest>
    {
        public Validator()
        {
            RuleFor(request => request.Page).GreaterThanOrEqualTo(1);
            RuleFor(request => request.PageSize).InclusiveBetween(1, 20);
        }
    }
}

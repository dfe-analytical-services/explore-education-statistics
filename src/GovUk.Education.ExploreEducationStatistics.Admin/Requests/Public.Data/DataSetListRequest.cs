#nullable enable
using FluentValidation;

namespace GovUk.Education.ExploreEducationStatistics.Admin.Requests.Public.Data;

public record DataSetListRequest
{
    /// <summary>
    /// The page of results to fetch.
    /// </summary>
    public int Page { get; init; } = 1;

    /// <summary>
    /// The maximum number of results per page.
    /// </summary>
    public int PageSize { get; init; } = 10;

    public class Validator : AbstractValidator<DataSetListRequest>
    {
        public Validator()
        {
            RuleFor(request => request.Page)
                .GreaterThanOrEqualTo(1);
            RuleFor(request => request.PageSize)
                .InclusiveBetween(1, 100);
        }
    }
}

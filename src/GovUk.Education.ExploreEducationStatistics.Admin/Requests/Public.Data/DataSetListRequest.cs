#nullable enable
using FluentValidation;

namespace GovUk.Education.ExploreEducationStatistics.Admin.Requests.Public.Data;

public record DataSetListRequest
{
    public Guid PublicationId { get; init; }

    public int Page { get; init; } = 1;

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

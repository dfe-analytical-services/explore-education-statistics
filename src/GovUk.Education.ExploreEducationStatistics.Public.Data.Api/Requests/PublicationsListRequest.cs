using FluentValidation;

namespace GovUk.Education.ExploreEducationStatistics.Public.Data.Api.Requests;

public record PublicationsListRequest(
    string? Search,
    int Page = 1,
    int PageSize = 10)
{
    public class Validator : AbstractValidator<PublicationsListRequest>
    {
        public Validator()
        {
            RuleFor(request => request.Search)
                .MinimumLength(3)
                .MaximumLength(100);
            RuleFor(request => request.Page)
                .GreaterThanOrEqualTo(1);
            RuleFor(request => request.PageSize)
                .GreaterThanOrEqualTo(1)
                .LessThanOrEqualTo(10);
        }
    }
}

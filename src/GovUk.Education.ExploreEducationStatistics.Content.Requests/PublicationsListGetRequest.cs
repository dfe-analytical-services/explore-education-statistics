using FluentValidation;
using GovUk.Education.ExploreEducationStatistics.Common.Model;
using GovUk.Education.ExploreEducationStatistics.Content.Model;

namespace GovUk.Education.ExploreEducationStatistics.Content.Requests;

public record PublicationsListGetRequest(
    ReleaseType? ReleaseType,
    Guid? ThemeId,
    string? Search,
    PublicationsSortBy? Sort,
    SortDirection? SortDirection,
    int Page = 1,
    int PageSize = 10
)
{
    public class Validator : AbstractValidator<PublicationsListGetRequest>
    {
        public Validator()
        {
            RuleFor(request => request.Search).MinimumLength(3);
            RuleFor(request => request.Page).InclusiveBetween(1, 9999);
            RuleFor(request => request.PageSize).InclusiveBetween(1, 40);
        }
    }
}

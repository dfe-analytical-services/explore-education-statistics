using FluentValidation;
using GovUk.Education.ExploreEducationStatistics.Common.Model;
using GovUk.Education.ExploreEducationStatistics.Content.Model;

namespace GovUk.Education.ExploreEducationStatistics.Content.Requests;

public record PublicationsListPostRequest(
    ReleaseType? ReleaseType = null,
    Guid? ThemeId = null,
    string? Search = null,
    PublicationsSortBy? Sort = null,
    SortDirection? SortDirection = null,
    int Page = 1,
    int PageSize = 10,
    IEnumerable<Guid>? PublicationIds = null
)
{
    public class Validator : AbstractValidator<PublicationsListPostRequest>
    {
        public Validator()
        {
            RuleFor(request => request.Search).MinimumLength(3);
            RuleFor(request => request.Page).InclusiveBetween(1, 9999);
            RuleFor(request => request.PageSize).InclusiveBetween(1, 40);
        }
    }
}

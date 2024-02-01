using FluentValidation;
using GovUk.Education.ExploreEducationStatistics.Common.Model;

namespace GovUk.Education.ExploreEducationStatistics.Content.Requests;

public record DataSetsListRequest(
    Guid? ThemeId = null,
    Guid? PublicationId = null,
    Guid? ReleaseId = null,
    bool? Latest = null,
    string? SearchTerm = null,
    DataSetsListRequestOrderBy? OrderBy = null,
    SortOrder? Sort = null,
    int Page = 1,
    int PageSize = 10)
{
    public class Validator : AbstractValidator<DataSetsListRequest>
    {
        public Validator()
        {
            RuleFor(request => request.SearchTerm)
                .MinimumLength(3);
            RuleFor(request => request.ReleaseId).NotEmpty()
                .When(request => request.OrderBy == DataSetsListRequestOrderBy.Natural);
            RuleFor(request => request.SearchTerm).NotEmpty()
                .When(request => request.OrderBy == DataSetsListRequestOrderBy.Relevance);
            RuleFor(request => request.Page)
                .InclusiveBetween(1, 9999);
            RuleFor(request => request.PageSize)
                .InclusiveBetween(1, 40);
        }
    }
}

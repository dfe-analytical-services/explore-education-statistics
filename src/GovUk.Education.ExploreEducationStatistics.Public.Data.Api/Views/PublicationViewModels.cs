using System.Text.Json.Serialization;

namespace GovUk.Education.ExploreEducationStatistics.Public.Data.Api.Views;

public record PublicationListViewModel(Guid Id, string Title, string Slug, string Summary, DateTimeOffset LastPublished);

public record PaginatedPublicationListViewModel : PaginatedListViewModel<PublicationListViewModel>
{
    public PaginatedPublicationListViewModel(
        List<PublicationListViewModel> results,
        int totalResults,
        int page,
        int pageSize)
        : base(
            results: results,
            totalResults: totalResults,
            page: page,
            pageSize: pageSize)
    {
    }

    [JsonConstructor]
    public PaginatedPublicationListViewModel(List<PublicationListViewModel> results, PagingViewModel paging)
        : base(results, paging)
    {
    }
}

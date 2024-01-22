using System.Text.Json.Serialization;

namespace GovUk.Education.ExploreEducationStatistics.Public.Data.Api.ViewModels;

public record PublicationListViewModel
{
    public required Guid Id { get; init; }
    
    public required string Title { get; init; }
    
    public required string Slug { get; init; }
    
    public required string Summary { get; init; }
    
    public required DateTimeOffset LastPublished { get; init; }
}

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

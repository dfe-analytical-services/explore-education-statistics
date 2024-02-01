using System.Text.Json.Serialization;

namespace GovUk.Education.ExploreEducationStatistics.Public.Data.Api.ViewModels;

public record PublicationSummaryViewModel
{
    /// <summary>
    /// The ID of the publication.
    /// </summary>
    public required Guid Id { get; init; }

    /// <summary>
    /// The title of the publication.
    /// </summary>
    public required string Title { get; init; }

    /// <summary>
    /// The URL slug of the publication.
    /// </summary>
    public required string Slug { get; init; }

    /// <summary>
    /// The summary of the publication.
    /// </summary>
    public required string Summary { get; init; }

    /// <summary>
    /// When the publication was last published.
    /// </summary>
    public required DateTimeOffset LastPublished { get; init; }
}

public record PaginatedPublicationListViewModel : PaginatedListViewModel<PublicationSummaryViewModel>
{
    public PaginatedPublicationListViewModel(
        List<PublicationSummaryViewModel> results,
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
    public PaginatedPublicationListViewModel(List<PublicationSummaryViewModel> results, PagingViewModel paging)
        : base(results, paging)
    {
    }
}

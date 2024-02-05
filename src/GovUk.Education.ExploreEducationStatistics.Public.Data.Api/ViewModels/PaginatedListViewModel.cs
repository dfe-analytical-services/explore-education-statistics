using System.Text.Json.Serialization;

namespace GovUk.Education.ExploreEducationStatistics.Public.Data.Api.ViewModels;

public abstract record PaginatedListViewModel<T>
{
    /// <summary>
    /// The list of results for this page.
    /// </summary>
    public List<T> Results { get; }

    /// <summary>
    /// Provides metadata for use in pagination.
    /// </summary>
    public PagingViewModel Paging { get; }

    public PaginatedListViewModel(List<T> results, int totalResults, int page, int pageSize)
    {
        Results = results;
        Paging = new PagingViewModel(page: page, pageSize: pageSize, totalResults: totalResults);
    }

    [JsonConstructor]
    public PaginatedListViewModel(List<T> results, PagingViewModel paging)
    {
        Results = results;
        Paging = paging;
    }
};

public record PagingViewModel
{
    /// <summary>
    /// The current page number.
    /// </summary>
    public int Page { get; }

    /// <summary>
    /// The maximum number of results per page.
    /// </summary>
    public int PageSize { get; }

    /// <summary>
    /// The total number of results across all pages.
    /// </summary>
    public int TotalResults { get; }

    /// <summary>
    /// The total number of pages.
    /// </summary>
    public int TotalPages => ((TotalResults - 1) / PageSize) + 1;

    public PagingViewModel(int page, int pageSize, int totalResults)
    {
        if (page < 1)
        {
            throw new ArgumentException("Page cannot be less than 1");
        }

        if (pageSize < 1)
        {
            throw new ArgumentException("Page size cannot be less than 1");
        }

        Page = page;
        PageSize = pageSize;
        TotalResults = totalResults;
    }
}

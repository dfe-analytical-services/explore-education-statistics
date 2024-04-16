using System.ComponentModel.DataAnnotations;

namespace GovUk.Education.ExploreEducationStatistics.Public.Data.Api.ViewModels;

public abstract record PaginatedListViewModel<T>
{
    /// <summary>
    /// Provides metadata for use in pagination.
    /// </summary>
    public required PagingViewModel Paging { get; init; }

    /// <summary>
    /// The list of results for this page.
    /// </summary>
    public required List<T> Results { get; init; }
}

public record PagingViewModel
{
    /// <summary>
    /// The current page number.
    /// </summary>
    [Required]
    public int Page { get; }

    /// <summary>
    /// The maximum number of results per page.
    /// </summary>
    [Required]
    public int PageSize { get; }

    /// <summary>
    /// The total number of results across all pages.
    /// </summary>
    [Required]
    public int TotalResults { get; }

    /// <summary>
    /// The total number of pages.
    /// </summary>
    [Required]
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

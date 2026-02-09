using System.ComponentModel.DataAnnotations;

namespace GovUk.Education.ExploreEducationStatistics.Public.Data.ViewModels;

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
    /// <example>1</example>
    [Required]
    public int Page { get; }

    /// <summary>
    /// The maximum number of results per page.
    /// </summary>
    /// <example>20</example>
    [Required]
    public int PageSize { get; }

    /// <summary>
    /// The total number of results across all pages.
    /// </summary>
    /// <example>60</example>
    [Required]
    public int TotalResults { get; }

    /// <summary>
    /// The total number of pages.
    /// </summary>
    /// <example>3</example>
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

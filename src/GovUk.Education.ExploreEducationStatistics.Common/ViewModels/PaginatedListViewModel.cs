#nullable enable
using System.Text.Json.Serialization;
using GovUk.Education.ExploreEducationStatistics.Common.Extensions;

namespace GovUk.Education.ExploreEducationStatistics.Common.ViewModels;

public record PaginatedListViewModel<T>
{
    public List<T> Results { get; }

    public PagingViewModel Paging { get; }

    public PaginatedListViewModel(List<T> results, int totalResults, int page, int pageSize)
    {
        Results = results;
        Paging = new PagingViewModel(page: page, pageSize: pageSize, totalResults: totalResults);
    }

    [JsonConstructor]
    [Newtonsoft.Json.JsonConstructor]
    public PaginatedListViewModel(List<T> results, PagingViewModel paging)
    {
        Results = results;
        Paging = paging;
    }

    /// <summary>
    /// Paginates a list of results (in memory) that have not been paginated yet.
    /// </summary>
    /// <remarks>
    /// Construct <see cref="PaginatedListViewModel{T}"/> directly when the results can be
    /// paginated beforehand i.e. in the database. This is more efficient as
    /// pagination should aim to avoid pulling all results into memory. 
    /// This method is intended for cases where it is not possible to do this.
    /// </remarks>
    /// <typeparam name="T">The type of elements in the source list</typeparam>
    /// <param name="allResults">The complete list of unpaginated results</param>
    /// <param name="page">The current page number</param>
    /// <param name="pageSize">The number of items per page</param>
    /// <returns>
    /// A <see cref="PaginatedListViewModel{T}"/> containing the elements of the specified page, along with pagination metadata.
    /// </returns>
    public static PaginatedListViewModel<T> Paginate(
        List<T> allResults,
        int page,
        int pageSize) =>
        new(
            results: allResults
                .Paginate(page, pageSize)
                .ToList(),
            totalResults: allResults.Count,
            page: page,
            pageSize: pageSize
        );
}

public record PagingViewModel
{
    public int Page { get; }

    public int PageSize { get; }

    public int TotalResults { get; }

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

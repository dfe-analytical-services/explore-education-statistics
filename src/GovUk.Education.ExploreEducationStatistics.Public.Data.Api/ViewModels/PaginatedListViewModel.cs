using System.Text.Json.Serialization;

namespace GovUk.Education.ExploreEducationStatistics.Public.Data.Api.ViewModels;

public abstract record PaginatedListViewModel<T>
{
    public List<T> Results { get; }

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

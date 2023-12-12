namespace GovUk.Education.ExploreEducationStatistics.Public.Data.Api.Views;

public record PaginatedListViewModel<T>(IReadOnlyList<T> Results, PagingViewModel Paging);

public record PagingViewModel(int Page, int PageSize, int TotalResults, int TotalPages);

namespace GovUk.Education.ExploreEducationStatistics.Content.Search.FunctionApp.Clients.ContentApi.Dtos;

public class PaginatedResult<T>
{
    public List<T> Results { get; init; }
    public PagingDto Paging { get; init; }
}
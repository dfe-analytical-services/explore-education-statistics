namespace GovUk.Education.ExploreEducationStatistics.Content.Search.FunctionApp.Clients.ContentApi.Dtos;

public record PagingDto
{
    public int Page { get; init; }

    public int PageSize { get; init; }

    public int TotalResults { get; init; }
    
    public int TotalPages { get; init; }
}

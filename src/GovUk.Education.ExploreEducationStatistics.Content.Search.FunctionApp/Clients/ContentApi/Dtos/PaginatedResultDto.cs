// ReSharper disable CollectionNeverUpdated.Global
// ReSharper disable ClassNeverInstantiated.Global
namespace GovUk.Education.ExploreEducationStatistics.Content.Search.FunctionApp.Clients.ContentApi.Dtos;

public record PaginatedResultDto<T>
{
    public List<T>? Results { get; init; }
    public PagingDto? Paging { get; init; }
}

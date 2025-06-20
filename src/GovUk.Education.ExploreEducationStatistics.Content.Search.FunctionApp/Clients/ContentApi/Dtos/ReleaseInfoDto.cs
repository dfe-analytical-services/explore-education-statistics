namespace GovUk.Education.ExploreEducationStatistics.Content.Search.FunctionApp.Clients.ContentApi.Dtos;

public record ReleaseInfoDto
{
    public Guid? ReleaseId { get; init; }
    public string? ReleaseSlug { get; init; }
}

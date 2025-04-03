namespace GovUk.Education.ExploreEducationStatistics.Content.Search.FunctionApp.Functions.OnReleaseVersionPublished.Dtos;

public record ReleaseVersionPublishedEventDto
{
    public Guid? ReleaseId {get;init;}
    public string? ReleaseSlug { get; init; }
    public Guid? PublicationId { get; init; }
    public string? PublicationSlug { get; init; }
    public Guid? PublicationLatestPublishedReleaseVersionId { get; init; }   
}

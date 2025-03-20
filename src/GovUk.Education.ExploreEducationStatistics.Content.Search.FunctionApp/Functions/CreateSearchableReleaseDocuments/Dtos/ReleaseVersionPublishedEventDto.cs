namespace GovUk.Education.ExploreEducationStatistics.Content.Search.FunctionApp.Functions.CreateSearchableReleaseDocuments.Dtos;

public record ReleaseVersionPublishedEventDto
{
    public required Guid ReleaseId {get;init;}
    public required string ReleaseSlug { get; init; }
    public required Guid PublicationId { get; init; }
    public required string PublicationSlug { get; init; }
    public required Guid PublicationLatestPublishedReleaseVersionId { get; init; }   
}

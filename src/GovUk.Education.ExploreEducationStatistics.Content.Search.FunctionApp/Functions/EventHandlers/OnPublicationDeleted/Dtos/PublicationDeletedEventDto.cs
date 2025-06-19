namespace GovUk.Education.ExploreEducationStatistics.Content.Search.FunctionApp.Functions.EventHandlers.OnPublicationDeleted.Dtos;

public record PublicationDeletedEventDto
{
    public string? PublicationSlug { get; init; }

    public Guid? LatestPublishedReleaseId { get; init; }

    public Guid? LatestPublishedReleaseVersionId { get; init; }
}

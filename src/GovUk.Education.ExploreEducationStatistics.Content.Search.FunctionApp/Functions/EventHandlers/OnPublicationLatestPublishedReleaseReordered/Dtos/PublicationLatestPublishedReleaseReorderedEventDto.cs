namespace GovUk.Education.ExploreEducationStatistics.Content.Search.FunctionApp.Functions.EventHandlers.OnPublicationLatestPublishedReleaseReordered.Dtos;

public class PublicationLatestPublishedReleaseReorderedEventDto
{
    public string? Title { get; init; }
    public string? Slug { get; init; }
    public Guid? LatestPublishedReleaseVersionId { get; init; }
    public Guid? PreviousReleaseVersionId { get; init; }
}

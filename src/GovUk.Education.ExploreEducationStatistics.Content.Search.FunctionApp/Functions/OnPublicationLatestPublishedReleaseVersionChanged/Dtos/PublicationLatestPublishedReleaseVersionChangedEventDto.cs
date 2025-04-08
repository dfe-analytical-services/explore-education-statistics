namespace GovUk.Education.ExploreEducationStatistics.Content.Search.FunctionApp.Functions.OnPublicationLatestPublishedReleaseVersionChanged.Dtos;

public class PublicationLatestPublishedReleaseVersionChangedEventDto
{
    public string? Title { get; init; }
    public string? Slug { get; init; }
    public Guid? LatestPublishedReleaseVersionId { get; init; }
    public Guid? PreviousReleaseVersionId { get; init; }
}

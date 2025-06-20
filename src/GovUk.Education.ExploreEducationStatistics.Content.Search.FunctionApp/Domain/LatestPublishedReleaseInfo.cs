namespace GovUk.Education.ExploreEducationStatistics.Content.Search.FunctionApp.Domain;

public record LatestPublishedReleaseInfo
{
    public required Guid LatestPublishedReleaseId { get; init; }

    public required Guid LatestPublishedReleaseVersionId { get; init; }
}

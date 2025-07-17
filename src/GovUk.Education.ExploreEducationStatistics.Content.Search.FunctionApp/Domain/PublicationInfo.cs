namespace GovUk.Education.ExploreEducationStatistics.Content.Search.FunctionApp.Domain;

public record PublicationInfo
{
    public required string PublicationSlug { get; init; }
    public required string LatestReleaseSlug { get; init; }
}

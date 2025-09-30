namespace GovUk.Education.ExploreEducationStatistics.Publisher.Services;

public record PublishedReleaseVersionInfo
{
    public required Guid ReleaseVersionId { get; init; }

    public required Guid ReleaseId { get; init; }

    public required string ReleaseSlug { get; init; }

    public required Guid PublicationId { get; init; }
}

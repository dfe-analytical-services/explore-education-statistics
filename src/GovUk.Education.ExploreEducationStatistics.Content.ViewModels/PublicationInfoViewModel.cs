using GovUk.Education.ExploreEducationStatistics.Content.Model;

namespace GovUk.Education.ExploreEducationStatistics.Content.ViewModels;

public record PublicationInfoViewModel
{
    public Guid PublicationId { get; init; }
    public required string PublicationSlug { get; init; }

    public required Guid? LatestPublishedReleaseId { get; init; }
    public required string? LatestPublishedReleaseSlug { get; init; }

    public static PublicationInfoViewModel FromEntity(Publication publication) =>
        new()
        {
            PublicationId = publication.Id,
            PublicationSlug = publication.Slug,
            LatestPublishedReleaseId = publication.LatestPublishedReleaseVersion?.ReleaseId,
            LatestPublishedReleaseSlug = publication.LatestPublishedReleaseVersion?.Release.Slug
        };
}

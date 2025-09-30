using GovUk.Education.ExploreEducationStatistics.Content.Model;

namespace GovUk.Education.ExploreEducationStatistics.Content.ViewModels;

public record PublicationInfoViewModel
{
    public Guid PublicationId { get; init; }
    public required string PublicationSlug { get; init; }

    public ReleaseInfoViewModel? LatestPublishedRelease { get; init; }

    public static PublicationInfoViewModel FromEntity(Publication publication) =>
        new()
        {
            PublicationId = publication.Id,
            PublicationSlug = publication.Slug,
            LatestPublishedRelease =
                publication.LatestPublishedReleaseVersion != null
                    ? new ReleaseInfoViewModel
                    {
                        ReleaseId = publication.LatestPublishedReleaseVersion.ReleaseId,
                        ReleaseSlug = publication.LatestPublishedReleaseVersion.Release.Slug
                    }
                    : null
        };
}

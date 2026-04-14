using GovUk.Education.ExploreEducationStatistics.Content.Model;

namespace GovUk.Education.ExploreEducationStatistics.Content.ViewModels;

/// <summary>
/// Response type used only by the Content.Search.FunctionApp (Search Docs Function App)
/// via the <c>GET /api/publicationInfos</c> endpoint.
/// </summary>
public record PublicationInfoViewModel
{
    public required Guid PublicationId { get; init; }
    public required string PublicationSlug { get; init; }

    public required ReleaseInfoViewModel? LatestPublishedRelease { get; init; }

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
                        ReleaseSlug = publication.LatestPublishedReleaseVersion.Release.Slug,
                    }
                    : null,
        };
}

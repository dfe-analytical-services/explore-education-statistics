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

    public required PublicationInfoReleaseViewModel LatestPublishedRelease { get; init; }

    public static PublicationInfoViewModel FromEntity(Publication publication) =>
        new()
        {
            PublicationId = publication.Id,
            PublicationSlug = publication.Slug,
            LatestPublishedRelease =
                publication.LatestPublishedReleaseVersion != null
                    ? PublicationInfoReleaseViewModel.FromEntity(publication.LatestPublishedReleaseVersion.Release)
                    : throw new InvalidOperationException("Publication must have a latest published release version"),
        };
}

public record PublicationInfoReleaseViewModel
{
    public required Guid ReleaseId { get; init; }
    public required string ReleaseSlug { get; init; }

    public static PublicationInfoReleaseViewModel FromEntity(Release release) =>
        new() { ReleaseId = release.Id, ReleaseSlug = release.Slug };
}

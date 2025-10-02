#nullable enable
namespace GovUk.Education.ExploreEducationStatistics.Content.Model.Predicates;

/// <summary>
/// <see cref="IQueryable{T}"/> extension methods for <see cref="ReleaseVersion"/>.
/// </summary>
public static class ReleaseVersionPredicates
{
    /// <summary>
    /// Filters a sequence of <see cref="IQueryable{T}"/> of type <see cref="ReleaseVersion"/> to only include the latest
    /// versions of each release.
    /// </summary>
    /// <param name="releaseVersions">The source <see cref="IQueryable{T}"/> of type <see cref="ReleaseVersion"/> to filter.</param>
    /// <param name="publicationId">Unique identifier of a publication to filter by.</param>
    /// <param name="releaseSlug">Slug of a release to filter by.</param>
    /// <param name="publishedOnly">Flag to only include published release versions.</param>
    /// <returns>An <see cref="IQueryable{T}"/> of type <see cref="ReleaseVersion"/> that contains elements from the input
    /// sequence filtered to only include the latest versions of each release.</returns>
    public static IQueryable<ReleaseVersion> LatestReleaseVersions(
        this IQueryable<ReleaseVersion> releaseVersions,
        Guid? publicationId = null,
        string? releaseSlug = null,
        bool publishedOnly = false
    )
    {
        if (releaseSlug != null && publicationId == null)
        {
            throw new ArgumentException("Cannot filter by release slug without a publication id");
        }

        var maxVersionsQueryable = releaseVersions
            .Where(releaseVersion => publicationId == null || releaseVersion.Release.PublicationId == publicationId)
            .Where(releaseVersion => releaseSlug == null || releaseVersion.Release.Slug == releaseSlug)
            .Where(releaseVersion => !publishedOnly || releaseVersion.Published.HasValue)
            .GroupBy(releaseVersion => releaseVersion.ReleaseId)
            .Select(groupedVersions => new
            {
                ReleaseId = groupedVersions.Key,
                Version = groupedVersions.Max(releaseVersion => releaseVersion.Version),
            });

        return releaseVersions.Join(
            maxVersionsQueryable,
            releaseVersion => new { releaseVersion.ReleaseId, releaseVersion.Version },
            maxVersion => maxVersion,
            (releaseVersion, _) => releaseVersion
        );
    }

    /// <summary>
    /// Filters a sequence of <see cref="IQueryable{T}"/> of type <see cref="ReleaseVersion"/> to only include the latest
    /// version of the release.
    /// </summary>
    /// <param name="releaseVersions">The source <see cref="IQueryable{T}"/> of type <see cref="ReleaseVersion"/> to filter.</param>
    /// <param name="releaseId">Unique identifier of a release to filter by.</param>
    /// <param name="publishedOnly">Flag to only include published release versions.</param>
    /// <param name="includeUnpublishedVersionIds">Optional list of unpublished release version ids to also consider.
    /// Applicable when <paramref name="publishedOnly"/> is true.</param>
    /// <returns>An <see cref="IQueryable{T}"/> of type <see cref="ReleaseVersion"/> that contains elements from the input
    /// sequence filtered to only include the latest version of the release.</returns>
    public static IQueryable<ReleaseVersion?> LatestReleaseVersion(
        this IQueryable<ReleaseVersion> releaseVersions,
        Guid releaseId,
        bool publishedOnly = false,
        IReadOnlyList<Guid>? includeUnpublishedVersionIds = null
    )
    {
        return releaseVersions
            .Where(releaseVersion => releaseVersion.ReleaseId == releaseId)
            .Where(releaseVersion =>
                releaseVersion.Version
                == releaseVersions
                    .Where(latestVersion => latestVersion.ReleaseId == releaseId)
                    .Where(latestVersion =>
                        !publishedOnly
                        || latestVersion.Published.HasValue
                        || (
                            includeUnpublishedVersionIds != null
                            && includeUnpublishedVersionIds.Contains(latestVersion.Id)
                        )
                    )
                    .Select(latestVersion => (int?)latestVersion.Version)
                    .Max()
            );
    }
}

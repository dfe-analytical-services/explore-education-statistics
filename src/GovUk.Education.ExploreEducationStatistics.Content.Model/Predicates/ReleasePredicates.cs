#nullable enable
using System;
using System.Linq;

namespace GovUk.Education.ExploreEducationStatistics.Content.Model.Predicates;

/// <summary>
/// <see cref="IQueryable{T}"/> extension methods for <see cref="Release"/>.
/// </summary>
public static class ReleasePredicates
{
    /// <summary>
    /// Filters a sequence of <see cref="IQueryable{T}"/> of type <see cref="Release"/> to only include the latest
    /// published versions of each release.
    /// </summary>
    /// <param name="releases">The source <see cref="IQueryable{T}"/> of type <see cref="Release"/> to filter.</param>
    /// <param name="publicationId">Unique identifier of a publication to filter by.</param>
    /// <param name="releaseSlug">Slug of a release to filter by.</param>
    /// <param name="publishedOnly">Flag to only include published release versions.</param>
    /// <returns>An <see cref="IQueryable{T}"/> of type <see cref="Release"/> that contains elements from the input
    /// sequence filtered to only include the latest versions of each release.</returns>
    public static IQueryable<Release> LatestReleaseVersions(this IQueryable<Release> releases,
        Guid? publicationId = null,
        string? releaseSlug = null,
        bool publishedOnly = false)
    {
        if (releaseSlug != null && publicationId == null)
        {
            throw new ArgumentException("Cannot filter by release slug without a publication id");
        }

        var maxVersionsQueryable = releases
            .Where(release => publicationId == null || release.PublicationId == publicationId)
            .Where(release => releaseSlug == null || release.Slug == releaseSlug)
            .Where(release => !publishedOnly || release.Published.HasValue)
            .GroupBy(release => release.ReleaseParentId)
            .Select(groupedReleases =>
                new
                {
                    ReleaseParentId = groupedReleases.Key, Version = groupedReleases.Max(release => release.Version)
                });

        return releases
            .Join(maxVersionsQueryable,
                release => new { release.ReleaseParentId, release.Version },
                maxVersion => maxVersion,
                (release, _) => release);
    }
}

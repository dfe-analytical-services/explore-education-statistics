#nullable enable
namespace GovUk.Education.ExploreEducationStatistics.Content.Model.Repository.Interfaces;

public interface IReleaseVersionRepository
{
    Task<DateTime> GetPublishedDate(
        Guid releaseVersionId,
        DateTime actualPublishedDate);

    /// <summary>
    /// Retrieves the latest published version of a release matching a given slug associated with a publication.
    /// </summary>
    /// <param name="publicationId">The unique identifier of the publication.</param>
    /// <param name="releaseSlug">The slug of the release.</param>
    /// <param name="cancellationToken">A <see cref="CancellationToken"/> to observe while waiting for the task to complete.</param>
    /// <returns>The latest published version of the release associated with the publication.</returns>
    Task<ReleaseVersion?> GetLatestPublishedReleaseVersionByReleaseSlug(
        Guid publicationId,
        string releaseSlug,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Retrieves the latest version of the latest release in release series order associated with a publication.
    /// </summary>
    /// <param name="publicationId">The unique identifier of the publication.</param>
    /// <param name="cancellationToken">A <see cref="CancellationToken"/> to observe while waiting for the task to complete.</param>
    /// <returns>The latest version of the latest release in release series order associated with the publication.</returns>
    Task<ReleaseVersion?> GetLatestReleaseVersion(
        Guid publicationId,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Determines if a release version is the latest published version of a release.
    /// </summary>
    /// <param name="releaseVersionId">The unique identifier of the release version.</param>
    /// <param name="cancellationToken">A <see cref="CancellationToken"/> to observe while waiting for the task to complete.</param>
    /// <returns>True if no newer published version of the release exists.</returns>
    Task<bool> IsLatestPublishedReleaseVersion(
        Guid releaseVersionId,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Determines if a release version is the latest version of a release.
    /// </summary>
    /// <param name="releaseVersionId">The unique identifier of the release version.</param>
    /// <param name="cancellationToken">A <see cref="CancellationToken"/> to observe while waiting for the task to complete.</param>
    /// <returns>True if no newer version of the release exists.</returns>
    Task<bool> IsLatestReleaseVersion(
        Guid releaseVersionId,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Retrieves the latest version id's of all releases associated with a publication.
    /// </summary>
    /// <param name="publicationId">The unique identifier of the publication.</param>
    /// <param name="publishedOnly">Flag to only include published release version id's.</param>
    /// <param name="cancellationToken">A <see cref="CancellationToken"/> to observe while waiting for the task to complete.</param>
    /// <returns>A collection of the latest version id's of all releases associated with the publication.</returns>
    Task<List<Guid>> ListLatestReleaseVersionIds(
        Guid publicationId,
        bool publishedOnly = false,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Retrieves the latest versions of all releases in release series order associated with a given publication.
    /// </summary>
    /// <param name="publicationId">The unique identifier of the publication.</param>
    /// <param name="publishedOnly">Flag to only include published release versions.</param>
    /// <param name="cancellationToken">A <see cref="CancellationToken"/> to observe while waiting for the task to complete.</param>
    /// <returns>A collection of the latest versions of all releases in release series order associated with the publication.</returns>
    Task<List<ReleaseVersion>> ListLatestReleaseVersions(
        Guid publicationId,
        bool publishedOnly = false,
        CancellationToken cancellationToken = default);
}

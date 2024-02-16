#nullable enable
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace GovUk.Education.ExploreEducationStatistics.Content.Model.Repository.Interfaces;

public interface IReleaseRepository
{
    Task<DateTime> GetPublishedDate(Guid releaseId,
        DateTime actualPublishedDate);

    /// <summary>
    /// Retrieves the latest published version from all releases in reverse chronological order that are associated with a publication.
    /// </summary>
    /// <param name="publicationId">The unique identifier of the publication.</param>
    /// <param name="cancellationToken">A <see cref="CancellationToken"/> to observe while waiting for the task to complete.</param>
    /// <returns>The latest published version from all releases in reverse chronological order that are associated with a publication.</returns>
    Task<Release?> GetLatestPublishedReleaseVersion(
        Guid publicationId,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Retrieves the latest published version of a release matching a given slug associated with a publication.
    /// </summary>
    /// <param name="publicationId">The unique identifier of the publication.</param>
    /// <param name="releaseSlug">The slug of the release.</param>
    /// <param name="cancellationToken">A <see cref="CancellationToken"/> to observe while waiting for the task to complete.</param>
    /// <returns>The latest published version of the release associated with the publication.</returns>
    Task<Release?> GetLatestPublishedReleaseVersion(
        Guid publicationId,
        string releaseSlug,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Retrieves the latest version from all releases in reverse chronological order that are associated with a publication.
    /// </summary>
    /// <param name="publicationId">The unique identifier of the publication.</param>
    /// <param name="cancellationToken">A <see cref="CancellationToken"/> to observe while waiting for the task to complete.</param>
    /// <returns>The latest version from all releases in reverse chronological order that are associated with a publication.</returns>
    Task<Release?> GetLatestReleaseVersion(
        Guid publicationId,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Determines if a release version is the latest published version of a release.
    /// </summary>
    /// <param name="releaseId">The unique identifier of the release version.</param>
    /// <param name="cancellationToken">A <see cref="CancellationToken"/> to observe while waiting for the task to complete.</param>
    /// <returns>True if no newer published version of the release exists.</returns>
    Task<bool> IsLatestPublishedReleaseVersion(
        Guid releaseId,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Determines if a release version is the latest version of a release.
    /// </summary>
    /// <param name="releaseId">The unique identifier of the release version.</param>
    /// <param name="cancellationToken">A <see cref="CancellationToken"/> to observe while waiting for the task to complete.</param>
    /// <returns>True if no newer version of the release exists.</returns>
    Task<bool> IsLatestReleaseVersion(
        Guid releaseId,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Retrieves the latest published version id's of all releases.
    /// </summary>
    /// <param name="cancellationToken">A <see cref="CancellationToken"/> to observe while waiting for the task to complete.</param>
    /// <returns>A collection of the latest published version id's of all releases.</returns>
    // TODO EES-4336 Remove this
    Task<List<Guid>> ListLatestPublishedReleaseVersionIds(CancellationToken cancellationToken = default);

    /// <summary>
    /// Retrieves the latest published release version id's associated with a publication in reverse chronological order.
    /// </summary>
    /// <param name="publicationId">The unique identifier of the publication.</param>
    /// <param name="cancellationToken">A <see cref="CancellationToken"/> to observe while waiting for the task to complete.</param>
    /// <returns>A collection of the latest published version id's of all releases associated with the publication.</returns>
    Task<List<Guid>> ListLatestPublishedReleaseVersionIds(
        Guid publicationId,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Retrieves the latest published versions of all releases associated with a publication in reverse chronological order.
    /// </summary>
    /// <param name="publicationId">The unique identifier of the publication.</param>
    /// <param name="cancellationToken">A <see cref="CancellationToken"/> to observe while waiting for the task to complete.</param>
    /// <returns>A collection of the latest published versions of all releases associated with the publication.</returns>
    Task<List<Release>> ListLatestPublishedReleaseVersions(
        Guid publicationId,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Retrieves the latest version id's of all releases associated with a publication in reverse chronological order.
    /// </summary>
    /// <param name="publicationId">The unique identifier of the publication.</param>
    /// <param name="cancellationToken">A <see cref="CancellationToken"/> to observe while waiting for the task to complete.</param>
    /// <returns>A collection of the latest version id's of all releases associated with the publication.</returns>
    Task<List<Guid>> ListLatestReleaseVersionIds(
        Guid publicationId,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Retrieves the latest versions of all releases in reverse chronological order.
    /// </summary>
    /// <param name="cancellationToken">A <see cref="CancellationToken"/> to observe while waiting for the task to complete.</param>
    /// <returns>A collection of the latest versions of all releases.</returns>
    Task<List<Release>> ListLatestReleaseVersions(CancellationToken cancellationToken = default);

    /// <summary>
    /// Retrieves the latest versions of all releases associated with a given publication in reverse chronological order.
    /// </summary>
    /// <param name="publicationId">The unique identifier of the publication.</param>
    /// <param name="cancellationToken">A <see cref="CancellationToken"/> to observe while waiting for the task to complete.</param>
    /// <returns>A collection of the latest version id's of all releases associated with the publication.</returns>
    Task<List<Release>> ListLatestReleaseVersions(
        Guid publicationId,
        CancellationToken cancellationToken = default);
}

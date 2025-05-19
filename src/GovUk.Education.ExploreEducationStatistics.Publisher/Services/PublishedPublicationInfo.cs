using System;
using System.Collections.Generic;
using System.Linq;

namespace GovUk.Education.ExploreEducationStatistics.Publisher.Services;

public record PublishedPublicationInfo
{
    public required Guid PublicationId { get; init; }

    public required string PublicationSlug { get; init; }

    /// <summary>
    /// The unique identifier of the latest published release for a publication, after the publishing run.
    /// </summary>
    public required Guid LatestPublishedReleaseId { get; init; }

    /// <summary>
    /// The unique identifier of the latest published release version for a publication, after the publishing run.
    /// </summary>
    public required Guid LatestPublishedReleaseVersionId { get; init; }

    /// <summary>
    /// The unique identifier of the previous latest published release for a publication, before the publishing run.
    /// This value is nullable, indicating that the publication may not have had a previously published release.
    /// </summary>
    public required Guid? PreviousLatestPublishedReleaseId { get; init; }

    /// <summary>
    /// The unique identifier of the previous latest published release version for a publication, before the publishing run.
    /// This value is nullable, indicating that the publication may not have had a previously published release.
    /// </summary>
    public required Guid? PreviousLatestPublishedReleaseVersionId { get; init; }

    /// <summary>
    /// The list of information about published release versions for a publication.
    /// </summary>
    public required IReadOnlyList<PublishedReleaseVersionInfo> PublishedReleaseVersions { get; init; }

    /// <summary>
    /// Indicates whether the publication was already published before the publishing run.
    /// </summary>
    ///
    public bool WasAlreadyPublished => PreviousLatestPublishedReleaseVersionId != null;

    public virtual bool Equals(PublishedPublicationInfo? other)
    {
        if (other is null)
        {
            return false;
        }

        if (ReferenceEquals(this, other))
        {
            return true;
        }

        return PublicationId.Equals(other.PublicationId) &&
               PublicationSlug == other.PublicationSlug &&
               LatestPublishedReleaseId.Equals(other.LatestPublishedReleaseId) &&
               LatestPublishedReleaseVersionId.Equals(other.LatestPublishedReleaseVersionId) &&
               Nullable.Equals(PreviousLatestPublishedReleaseId, other.PreviousLatestPublishedReleaseId) &&
               Nullable.Equals(PreviousLatestPublishedReleaseVersionId,
                   other.PreviousLatestPublishedReleaseVersionId) &&
               PublishedReleaseVersions.SequenceEqual(other.PublishedReleaseVersions);
    }

    public override int GetHashCode() => HashCode.Combine(
        PublicationId,
        PublicationSlug,
        LatestPublishedReleaseId,
        LatestPublishedReleaseVersionId,
        PreviousLatestPublishedReleaseId,
        PreviousLatestPublishedReleaseVersionId,
        PublishedReleaseVersions);
}

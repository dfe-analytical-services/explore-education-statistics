using System;

namespace GovUk.Education.ExploreEducationStatistics.Publisher.Services;

internal record PublishedPublicationInfo
{
    public required Guid PublicationId { get; init; }

    public required string PublicationSlug { get; init; }

    /// <summary>
    /// The unique identifier of the initial latest published release version for a publication, before the publishing run.
    /// This value is nullable, indicating that the publication may not have had a previously published release.
    /// </summary>
    public required Guid? InitialLatestPublishedReleaseVersionId { get; init; }

    /// <summary>
    /// The unique identifier of the latest published release version for a publication after the publishing run.
    /// </summary>
    public required Guid LatestPublishedReleaseVersionId { get; init; }

    /// <summary>
    /// Indicates whether the publication was already published before the publishing run.
    /// </summary>
    ///
    public bool WasAlreadyPublished => InitialLatestPublishedReleaseVersionId != null;
}

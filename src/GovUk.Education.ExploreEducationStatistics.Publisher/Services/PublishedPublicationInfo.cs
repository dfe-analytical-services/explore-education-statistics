using System;
using System.Collections.Generic;
using Generator.Equals;

namespace GovUk.Education.ExploreEducationStatistics.Publisher.Services;

[Equatable]
public partial record PublishedPublicationInfo
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
    [UnorderedEquality]
    public required IReadOnlyList<PublishedReleaseVersionInfo> PublishedReleaseVersions { get; init; }

    /// <summary>
    /// Indicates whether the publication was already published before the publishing run.
    /// </summary>
    ///
    public bool WasAlreadyPublished => PreviousLatestPublishedReleaseVersionId != null;
    
    /// <summary>
    /// Indicates whether the publication is archived/superseded
    /// </summary>
    public required bool IsPublicationArchived { get; init; }
}

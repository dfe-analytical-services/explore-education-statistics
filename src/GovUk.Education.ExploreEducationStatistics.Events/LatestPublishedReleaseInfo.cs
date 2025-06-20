namespace GovUk.Education.ExploreEducationStatistics.Events;

/// <summary>
/// Information about the latest published release associated with a publication.
/// </summary>
public record LatestPublishedReleaseInfo
{
    /// <summary>
    /// The unique identifier of the latest published release associated with a publication.
    /// </summary>
    public required Guid LatestPublishedReleaseId { get; init; }

    /// <summary>
    /// The unique identifier of the latest published release version associated with a publication.
    /// </summary>
    public required Guid LatestPublishedReleaseVersionId { get; init; }
}

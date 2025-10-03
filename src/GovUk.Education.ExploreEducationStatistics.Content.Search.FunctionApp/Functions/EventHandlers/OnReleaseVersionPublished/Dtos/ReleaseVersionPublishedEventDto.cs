namespace GovUk.Education.ExploreEducationStatistics.Content.Search.FunctionApp.Functions.EventHandlers.OnReleaseVersionPublished.Dtos;

public record ReleaseVersionPublishedEventDto
{
    /// <summary>
    /// Newly published release version id
    /// </summary>
    public Guid ReleaseVersionId { get; init; }

    /// <summary>
    /// The release id for the newly published release version
    /// </summary>
    public Guid? ReleaseId { get; init; }

    /// <summary>
    /// The release slug for the newly published release version
    /// </summary>
    public string? ReleaseSlug { get; init; }

    /// <summary>
    /// The publication id for the newly published release version
    /// </summary>
    public Guid? PublicationId { get; init; }

    /// <summary>
    /// The publication slug for the newly published release version
    /// </summary>
    public string? PublicationSlug { get; init; }

    /// <summary>
    /// The published release version might not belong to the publication's latest published release.
    /// This property contains the publication's latest published release id.
    /// </summary>
    public Guid? LatestPublishedReleaseId { get; init; }

    /// <summary>
    /// The published release version might not belong to the publication's latest published release.
    /// This property contains the latest published release version id of the publication's latest published release.
    /// </summary>
    public Guid? LatestPublishedReleaseVersionId { get; init; }

    /// <summary>
    /// The publication's latest published release id before the release version was published.
    /// </summary>
    public Guid? PreviousLatestPublishedReleaseId { get; init; }

    /// <summary>
    /// The latest published release version id of the publication's latest published release before the release version was published.
    /// </summary>
    public Guid? PreviousLatestPublishedReleaseVersionId { get; init; }

    /// <summary>
    /// Is this newly published release version the new latest version?
    /// </summary>
    public bool NewlyPublishedReleaseVersionIsLatest => LatestPublishedReleaseVersionId == ReleaseVersionId;

    /// <summary>
    /// Has a different release become the new latest?
    /// </summary>
    public bool NewlyPublishedReleaseVersionIsForDifferentRelease => PreviousLatestPublishedReleaseId != ReleaseId;

    /// <summary>
    /// Indicates whether the associated publication is archived
    /// </summary>
    public bool? IsPublicationArchived { get; init; }
}

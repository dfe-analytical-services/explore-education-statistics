namespace GovUk.Education.ExploreEducationStatistics.Content.Search.FunctionApp.Functions.EventHandlers.OnReleaseVersionPublished.Dtos;

public record ReleaseVersionPublishedEventDto
{
    /// <summary>
    /// Newly published release version id
    /// </summary>
    public Guid ReleaseVersionId { get; init; }
    
    /// <summary>
    /// The Release Id for the newly published release version
    /// </summary>
    public Guid? ReleaseId {get;init;}
    
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
    /// The Release Version Id of the current "latest release version".
    /// </summary>
    public Guid? LatestPublishedReleaseVersionId { get; init; }
    
    /// <summary>
    /// The Release Id of the previous "latest release version"
    /// </summary>
    public Guid? PreviousLatestPublishedReleaseId { get; init; }
    
    /// <summary>
    /// Is this newly published release version the new latest version?
    /// </summary>
    public bool NewlyPublishedReleaseVersionIsLatest => LatestPublishedReleaseVersionId == ReleaseVersionId;
    
    /// <summary>
    /// Has a different release become the new latest?
    /// </summary>
    public bool NewlyPublishedReleaseVersionIsForDifferentRelease => PreviousLatestPublishedReleaseId != ReleaseId;
}

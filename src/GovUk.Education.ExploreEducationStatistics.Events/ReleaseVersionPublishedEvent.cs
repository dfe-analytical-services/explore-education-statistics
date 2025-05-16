using Azure.Messaging.EventGrid;
using GovUk.Education.ExploreEducationStatistics.Events.EventGrid;

namespace GovUk.Education.ExploreEducationStatistics.Events;

public record ReleaseVersionPublishedEvent : IEvent
{
    public ReleaseVersionPublishedEvent(PublishedReleaseVersionInfo publishedReleaseVersion)
    {
        Subject = publishedReleaseVersion.ReleaseVersionId.ToString();
        Payload = new()
        {
            ReleaseId = publishedReleaseVersion.ReleaseId,
            ReleaseSlug = publishedReleaseVersion.ReleaseSlug,
            PublicationId = publishedReleaseVersion.PublicationId,
            PublicationSlug = publishedReleaseVersion.PublicationSlug,
            PublicationLatestPublishedReleaseVersionId = publishedReleaseVersion.PublicationLatestPublishedReleaseVersionId,
            PreviousLatestReleaseId = publishedReleaseVersion.PreviousLatestReleaseId,
        };
    }

    // Changes to this event should also increment the version accordingly.
    public const string DataVersion = "1.0";
    public const string EventType = "release-version-published";
    
    // Which Topic endpoint to use from the appsettings
    public static string EventTopicOptionsKey => "ReleaseVersionChangedEvent";
    
    /// <summary>
    /// The ReleaseVersionId is the subject
    /// </summary>
    public string Subject { get; }

    /// <summary>
    /// The event payload
    /// </summary>
    public record EventPayload
    {
        public required Guid ReleaseId {get;init;}
        public required string ReleaseSlug { get; init; }
        public required Guid PublicationId { get; init; }
        public required string PublicationSlug { get; init; }
        public required Guid PublicationLatestPublishedReleaseVersionId { get; init; }    
        public Guid? PreviousLatestReleaseId { get; init; }
    }
    public EventPayload Payload { get; }
    
    public EventGridEvent ToEventGridEvent() => new(Subject, EventType, DataVersion, Payload);

    public record PublishedReleaseVersionInfo
    {
        /// <summary>
        /// Newly published release version id
        /// </summary>
        public Guid ReleaseVersionId { get; init; }
    
        /// <summary>
        /// The Release Id for the newly published release version
        /// </summary>
        public Guid ReleaseId {get;init;}
        
        /// <summary>
        /// The release slug for the newly published release version
        /// </summary>
        public string ReleaseSlug { get; init; } = string.Empty;
        
        /// <summary>
        /// The publication id for the newly published release version
        /// </summary>
        public Guid PublicationId { get; init; }
        
        /// <summary>
        /// The publication slug for the newly published release version
        /// </summary>
        public string PublicationSlug { get; init; } = string.Empty;
        
        /// <summary>
        /// The release version that has been published may not be the latest.
        /// This property contains the latest published release version id.
        /// </summary>
        public Guid PublicationLatestPublishedReleaseVersionId { get; init; }
        
        /// <summary>
        /// The Release Id of the previous "latest release version"
        /// </summary>
        public Guid? PreviousLatestReleaseId { get; init; }
    }
}

using Azure.Messaging.EventGrid;
using GovUk.Education.ExploreEducationStatistics.Events.EventGrid;

namespace GovUk.Education.ExploreEducationStatistics.Events;

public record ReleaseVersionPublishedEvent : IEvent
{
    public ReleaseVersionPublishedEvent(ReleaseVersionPublishedEventInfo eventInfo)
    {
        Subject = eventInfo.ReleaseVersionId.ToString();
        Payload = new EventPayload
        {
            ReleaseId = eventInfo.ReleaseId,
            ReleaseSlug = eventInfo.ReleaseSlug,
            PublicationId = eventInfo.PublicationId,
            PublicationSlug = eventInfo.PublicationSlug,
            LatestPublishedReleaseId = eventInfo.LatestPublishedReleaseId,
            LatestPublishedReleaseVersionId = eventInfo.LatestPublishedReleaseVersionId,
            PreviousLatestPublishedReleaseId = eventInfo.PreviousLatestPublishedReleaseId,
            PreviousLatestPublishedReleaseVersionId = eventInfo.PreviousLatestPublishedReleaseVersionId
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
        public required Guid ReleaseId { get; init; }
        public required string ReleaseSlug { get; init; }
        public required Guid PublicationId { get; init; }
        public required string PublicationSlug { get; init; }
        public required Guid LatestPublishedReleaseId { get; init; }
        public required Guid LatestPublishedReleaseVersionId { get; init; }
        public Guid? PreviousLatestPublishedReleaseId { get; init; }
        public Guid? PreviousLatestPublishedReleaseVersionId { get; init; }
    }
    public EventPayload Payload { get; }
    
    public EventGridEvent ToEventGridEvent() => new(Subject, EventType, DataVersion, Payload);

    public record ReleaseVersionPublishedEventInfo
    {
        /// <summary>
        /// Newly published release version id
        /// </summary>
        public required Guid ReleaseVersionId { get; init; }

        /// <summary>
        /// The Release Id for the newly published release version
        /// </summary>
        public required Guid ReleaseId { get; init; }

        /// <summary>
        /// The release slug for the newly published release version
        /// </summary>
        public required string ReleaseSlug { get; init; }

        /// <summary>
        /// The publication id for the newly published release version
        /// </summary>
        public required Guid PublicationId { get; init; }

        /// <summary>
        /// The publication slug for the newly published release version
        /// </summary>
        public required string PublicationSlug { get; init; }

        /// <summary>
        /// </summary>
        public required Guid LatestPublishedReleaseId { get; init; }

        /// <summary>
        /// The release version that has been published may not be the latest.
        /// This property contains the latest published release version id.
        /// </summary>
        public required Guid LatestPublishedReleaseVersionId { get; init; }

        /// <summary>
        /// The Release Id of the previous "latest release version"
        /// </summary>
        public required Guid? PreviousLatestPublishedReleaseId { get; init; }

        /// <summary>
        /// </summary>
        public required Guid? PreviousLatestPublishedReleaseVersionId { get; init; }
    }
}

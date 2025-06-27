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
            ReleaseVersionId = eventInfo.ReleaseVersionId,
            ReleaseSlug = eventInfo.ReleaseSlug,
            PublicationId = eventInfo.PublicationId,
            PublicationSlug = eventInfo.PublicationSlug,
            LatestPublishedReleaseId = eventInfo.LatestPublishedReleaseId,
            LatestPublishedReleaseVersionId = eventInfo.LatestPublishedReleaseVersionId,
            PreviousLatestPublishedReleaseId = eventInfo.PreviousLatestPublishedReleaseId,
            PreviousLatestPublishedReleaseVersionId = eventInfo.PreviousLatestPublishedReleaseVersionId,
            IsPublicationArchived = eventInfo.IsPublicationArchived
        };
    }

    // Changes to this event should also increment the version accordingly.
    public const string DataVersion = "1.0";
    public const string EventType = ReleaseVersionChangedEventTypes.ReleaseVersionPublished;

    // Which Topic endpoint to use from the appsettings
    public static string EventTopicOptionsKey => EventTopicOptionsKeys.ReleaseVersionChanged;

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
        public required Guid ReleaseVersionId { get; set; }
        public required string ReleaseSlug { get; init; }
        public required Guid PublicationId { get; init; }
        public required string PublicationSlug { get; init; }
        public required Guid LatestPublishedReleaseId { get; init; }
        public required Guid LatestPublishedReleaseVersionId { get; init; }
        public required Guid? PreviousLatestPublishedReleaseId { get; init; }
        public required Guid? PreviousLatestPublishedReleaseVersionId { get; init; }
        public required bool IsPublicationArchived { get; init; }
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
        /// The release id for the newly published release version
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
        /// The published release version might not belong to the publication's latest published release.
        /// This property contains the publication's latest published release id.
        /// </summary>
        public required Guid LatestPublishedReleaseId { get; init; }

        /// <summary>
        /// The published release version might not belong to the publication's latest published release.
        /// This property contains the latest published release version id of the publication's latest published release.
        /// </summary>
        public required Guid LatestPublishedReleaseVersionId { get; init; }

        /// <summary>
        /// The publication's latest published release id before the release version was published.
        /// </summary>
        public required Guid? PreviousLatestPublishedReleaseId { get; init; }

        /// <summary>
        /// The latest published release version id of the publication's latest published release before the release version was published.
        /// </summary>
        public required Guid? PreviousLatestPublishedReleaseVersionId { get; init; }
        
        /// <summary>
        /// Indicates whether the associated publication is archived
        /// </summary>
        public required bool IsPublicationArchived { get; init; }
    }
}

using Azure.Messaging.EventGrid;
using GovUk.Education.ExploreEducationStatistics.Events.EventGrid;

namespace GovUk.Education.ExploreEducationStatistics.Events;

public record PublicationDeletedEvent : IEvent
{
    public PublicationDeletedEvent(
        Guid publicationId,
        string publicationSlug,
        LatestPublishedReleaseInfo? latestPublishedRelease,
        IReadOnlyList<Guid> releaseIds
    )
    {
        Subject = publicationId.ToString();
        Payload = new EventPayload
        {
            PublicationSlug = publicationSlug,
            LatestPublishedRelease = latestPublishedRelease,
            ReleaseIds = releaseIds,
        };
    }

    // Changes to this event should also increment the version accordingly.
    private const string DataVersion = "1.1";
    private const string EventType = PublicationChangedEventTypes.PublicationDeleted;

    // Which Topic endpoint to use from the appsettings
    public static string EventTopicOptionsKey => EventTopicOptionsKeys.PublicationChanged;

    /// <summary>
    /// The PublicationId is the subject
    /// </summary>
    public string Subject { get; }

    /// <summary>
    /// The event payload
    /// </summary>
    public EventPayload Payload { get; }

    public record EventPayload
    {
        public required string PublicationSlug { get; init; }
        public required LatestPublishedReleaseInfo? LatestPublishedRelease { get; init; }

        /// <summary>
        /// The ids of every Release that belonged to the deleted Publication.
        /// </summary>
        /// <remarks>
        /// A subscriber cannot look these up once the Publication is gone, and a searchable document is
        /// named after the Release it was built from, so they have to travel with the event.
        /// </remarks>
        public required IReadOnlyList<Guid> ReleaseIds { get; init; }
    }

    public EventGridEvent ToEventGridEvent() => new(Subject, EventType, DataVersion, Payload);
}

using Azure.Messaging.EventGrid;
using GovUk.Education.ExploreEducationStatistics.Events.EventGrid;

namespace GovUk.Education.ExploreEducationStatistics.Events;

public record PublicationRestoredEvent : IEvent
{
    public PublicationRestoredEvent(
        Guid publicationId,
        string publicationSlug,
        Guid previousSupersededByPublicationId)
    {
        Subject = publicationId.ToString();
        Payload = new EventPayload
        {
            PublicationSlug = publicationSlug,
            PreviousSupersededByPublicationId = previousSupersededByPublicationId
        };
    }

    // Changes to this event should also increment the version accordingly.
    private const string DataVersion = "1.0";
    private const string EventType = "publication-restored";

    // Which Topic endpoint to use from the appsettings
    public static string EventTopicOptionsKey => "PublicationChangedEvent";

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

        public required Guid PreviousSupersededByPublicationId { get; init; }
    }

    public EventGridEvent ToEventGridEvent() => new(Subject, EventType, DataVersion, Payload);
}

using Azure.Messaging.EventGrid;
using GovUk.Education.ExploreEducationStatistics.Events.EventGrid;

namespace GovUk.Education.ExploreEducationStatistics.Events;

public record PublicationArchivedEvent : IEvent
{
    public PublicationArchivedEvent(
        Guid publicationId,
        string publicationSlug,
        Guid supersededByPublicationId)
    {
        Subject = publicationId.ToString();
        Payload = new EventPayload
        {
            PublicationSlug = publicationSlug,
            SupersededByPublicationId = supersededByPublicationId
        };
    }

    // Changes to this event should also increment the version accordingly.
    private const string DataVersion = "1.0";
    private const string EventType = PublicationChangedEventTypes.PublicationArchived;

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

        public required Guid SupersededByPublicationId { get; init; }
    }

    public EventGridEvent ToEventGridEvent() => new(Subject, EventType, DataVersion, Payload);
}

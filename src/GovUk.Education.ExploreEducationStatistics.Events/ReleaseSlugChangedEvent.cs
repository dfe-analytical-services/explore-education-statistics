using Azure.Messaging.EventGrid;
using GovUk.Education.ExploreEducationStatistics.Events.EventGrid;

namespace GovUk.Education.ExploreEducationStatistics.Events;

public record ReleaseSlugChangedEvent : IEvent
{
    public ReleaseSlugChangedEvent(
        Guid releaseId,
        string newReleaseSlug,
        Guid publicationId,
        string publicationSlug)
    {
        Subject = releaseId.ToString();
        Payload = new EventPayload
        {
            NewReleaseSlug = newReleaseSlug,
            PublicationId = publicationId.ToString(),
            PublicationSlug = publicationSlug
        };
    }

    // Changes to this event should also increment the version accordingly.
    private const string DataVersion = "1.0";
    private const string EventType = ReleaseChangedEventTypes.ReleaseSlugChanged;

    // Which Topic endpoint to use from the appsettings
    public static string EventTopicOptionsKey => EventTopicOptionsKeys.ReleaseChanged;

    /// <summary>
    /// The ThemeId is the subject
    /// </summary>
    public string Subject { get; }

    /// <summary>
    /// The event payload
    /// </summary>
    public EventPayload Payload { get; }
    
    public record EventPayload
    {
        public required string NewReleaseSlug { get; init; }
        public required string PublicationId { get; init; }
        public required string PublicationSlug { get; init; }
    }

    public EventGridEvent ToEventGridEvent() => new(Subject, EventType, DataVersion, Payload);
}

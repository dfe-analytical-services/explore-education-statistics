using Azure.Messaging.EventGrid;
using GovUk.Education.ExploreEducationStatistics.Events.EventGrid;

namespace GovUk.Education.ExploreEducationStatistics.Events;

public record PublicationChangedEvent : IEvent
{
    public PublicationChangedEvent(
        Guid publicationId,
        string publicationSlug,
        string publicationTitle,
        string publicationSummary,
        bool isPublicationArchived
    )
    {
        Subject = publicationId.ToString();
        Payload = new EventPayload
        {
            Title = publicationTitle,
            Summary = publicationSummary,
            Slug = publicationSlug,
            IsPublicationArchived = isPublicationArchived,
        };
    }

    // Changes to this event should also increment the version accordingly.
    private const string DataVersion = "1.0";
    private const string EventType = PublicationChangedEventTypes.PublicationChanged;

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
        public required string Title { get; init; }
        public required string Summary { get; init; }
        public required string Slug { get; init; }
        public required bool IsPublicationArchived { get; init; }
    }

    public EventGridEvent ToEventGridEvent() => new(Subject, EventType, DataVersion, Payload);
}

using Azure.Messaging.EventGrid;
using GovUk.Education.ExploreEducationStatistics.Events.EventGrid;

namespace GovUk.Education.ExploreEducationStatistics.Events;

public record ThemeChangedEvent : IEvent
{
    public ThemeChangedEvent(
        Guid themeId,
        string themeTitle,
        string themeSummary,
        string themeSlug)
    {
        Subject = themeId.ToString();
        Payload = new EventPayload
        {
            Title = themeTitle,
            Summary = themeSummary,
            Slug = themeSlug
        };
    }

    // Changes to this event should also increment the version accordingly.
    private const string DataVersion = "1.0";
    private const string EventType = ThemeChangedEventTypes.ThemeChanged;

    // Which Topic endpoint to use from the appsettings
    public static string EventTopicOptionsKey => EventTopicOptionsKeys.ThemeChanged;

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
        public required string Title { get; init; }
        public required string Summary { get; init; }
        public required string Slug { get; init; }
    }

    public EventGridEvent ToEventGridEvent() => new(Subject, EventType, DataVersion, Payload);
}

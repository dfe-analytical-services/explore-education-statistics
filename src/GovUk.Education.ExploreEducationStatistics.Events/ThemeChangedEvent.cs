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
    private const string EventType = "theme-changed";
    
    // Which Topic endpoint to use from the appsettings
    public static string EventTopicOptionsKey => "ThemeChangedEvent";

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
        public string Title { get; init; }
        public string Summary { get; init; }
        public string Slug { get; init; }
    }

    public EventGridEvent ToEventGridEvent() => new(Subject, EventType, DataVersion, Payload);
}

using Azure.Messaging.EventGrid;
using GovUk.Education.ExploreEducationStatistics.Content.Model;

namespace GovUk.Education.ExploreEducationStatistics.Admin.Events;

public record ThemeChangedEventDto
{
    public ThemeChangedEventDto(Theme theme)
    {
        Subject = theme.Id.ToString();
        Payload = new EventPayload
        {
            Title = theme.Title,
            Summary = theme.Summary,
            Slug = theme.Slug
        };
    }

    // Changes to this event should also increment the version accordingly.
    private const string DataVersion = "1.0";
    private const string EventType = "theme-changed";
    
    // Which Topic endpoint to use from the appsettings
    public const string EventTopicOptionsKey = "ThemeChangesEvent";

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

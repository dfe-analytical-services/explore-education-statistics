using System;
using Azure.Messaging.EventGrid;

namespace GovUk.Education.ExploreEducationStatistics.Admin.Events;

public record ReleaseSlugChangedEventDto
{
    public ReleaseSlugChangedEventDto(Guid releaseId, string newReleaseSlug, Guid publicationId, string publicationSlug)
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
    private const string EventType = "release-slug-changed";
    
    // Which Topic endpoint to use from the appsettings
    public const string EventTopicOptionsKey = "ReleaseChangesEvent";

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
        public string NewReleaseSlug { get; init; }
        public string PublicationId { get; init; }
        public string PublicationSlug { get; init; }
    }

    public EventGridEvent ToEventGridEvent() => new(Subject, EventType, DataVersion, Payload);
}

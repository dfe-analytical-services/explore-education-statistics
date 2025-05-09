using Azure.Messaging.EventGrid;
using GovUk.Education.ExploreEducationStatistics.Events.EventGrid;

namespace GovUk.Education.ExploreEducationStatistics.Events;

public record PublicationLatestPublishedReleaseReorderedEvent : IEvent
{
    public PublicationLatestPublishedReleaseReorderedEvent(
        Guid publicationId,
        string publicationTitle,
        string publicationSlug,
        Guid latestPublishedReleaseVersionId,
        Guid previousReleaseVersionId)
    {
        Subject = publicationId.ToString();
        Payload = new EventPayload
        {
            Title = publicationTitle,
            Slug = publicationSlug,
            LatestPublishedReleaseVersionId = latestPublishedReleaseVersionId,
            PreviousReleaseVersionId = previousReleaseVersionId
        };
    }

    // Changes to this event should also increment the version accordingly.
    private const string DataVersion = "1.0";
    private const string EventType = "publication-latest-published-release-reordered";
    
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
        public string Title { get; init; }
        public string Slug { get; init; }
        public Guid LatestPublishedReleaseVersionId { get; init; }
        public Guid PreviousReleaseVersionId { get; init; }
    }

    public EventGridEvent ToEventGridEvent() => new(Subject, EventType, DataVersion, Payload);
}

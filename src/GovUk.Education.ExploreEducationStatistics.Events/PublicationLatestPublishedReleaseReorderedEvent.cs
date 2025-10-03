using Azure.Messaging.EventGrid;
using GovUk.Education.ExploreEducationStatistics.Events.EventGrid;

namespace GovUk.Education.ExploreEducationStatistics.Events;

public record PublicationLatestPublishedReleaseReorderedEvent : IEvent
{
    public PublicationLatestPublishedReleaseReorderedEvent(
        Guid publicationId,
        string publicationTitle,
        string publicationSlug,
        Guid latestPublishedReleaseId,
        Guid latestPublishedReleaseVersionId,
        Guid previousReleaseId,
        Guid previousReleaseVersionId,
        bool isPublicationArchived
    )
    {
        Subject = publicationId.ToString();
        Payload = new EventPayload
        {
            Title = publicationTitle,
            Slug = publicationSlug,
            LatestPublishedReleaseId = latestPublishedReleaseId,
            LatestPublishedReleaseVersionId = latestPublishedReleaseVersionId,
            PreviousReleaseId = previousReleaseId,
            PreviousReleaseVersionId = previousReleaseVersionId,
            IsPublicationArchived = isPublicationArchived,
        };
    }

    // Changes to this event should also increment the version accordingly.
    private const string DataVersion = "1.0";
    private const string EventType = PublicationChangedEventTypes.PublicationLatestPublishedReleaseReordered;

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
        public required string Slug { get; init; }
        public required Guid LatestPublishedReleaseId { get; init; }
        public required Guid LatestPublishedReleaseVersionId { get; init; }
        public required Guid PreviousReleaseId { get; init; }
        public required Guid PreviousReleaseVersionId { get; init; }
        public required bool IsPublicationArchived { get; set; }
    }

    public EventGridEvent ToEventGridEvent() => new(Subject, EventType, DataVersion, Payload);
}

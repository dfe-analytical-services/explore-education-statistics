using System;
using Azure.Messaging.EventGrid;
using GovUk.Education.ExploreEducationStatistics.Common.Services.EventGrid;
using GovUk.Education.ExploreEducationStatistics.Content.Model;

namespace GovUk.Education.ExploreEducationStatistics.Admin.Events;

public record PublicationLatestPublishedReleaseReorderedEvent : IEvent
{
    public PublicationLatestPublishedReleaseReorderedEvent(
        Publication publication,
        Guid previousReleaseVersionId)
    {
        Subject = publication.Id.ToString();
        Payload = new EventPayload
        {
            Title = publication.Title,
            Slug = publication.Slug,
            LatestPublishedReleaseVersionId = publication.LatestPublishedReleaseVersionId ?? throw new ArgumentNullException(nameof(publication.LatestPublishedReleaseVersionId)),
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

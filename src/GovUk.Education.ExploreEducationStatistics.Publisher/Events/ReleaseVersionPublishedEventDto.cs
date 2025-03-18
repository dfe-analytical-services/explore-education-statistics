using System;
using Azure.Messaging.EventGrid;

namespace GovUk.Education.ExploreEducationStatistics.Publisher.Events;

public record ReleaseVersionPublishedEventDto(Guid ReleaseVersionId, ReleaseVersionPublishedEventDto.EventPayload Payload)
{
    // Changes to this event should also increment the version accordingly.
    public const string DataVersion = "1.0";
    public const string EventType = "release-version-published";
    
    // Which Topic endpoint to use from the appsettings
    public const string EventTopicOptionsKey = "ReleaseVersionChangesEvent";
    
    /// <summary>
    /// The ReleaseVersionId is the subject
    /// </summary>
    public string Subject => ReleaseVersionId.ToString();

    /// <summary>
    /// The event payload
    /// </summary>
    public record EventPayload
    {
        public required Guid ReleaseId {get;init;}
        public required string ReleaseSlug { get; init; }
        public required Guid PublicationId { get; init; }
        public required string PublicationSlug { get; init; }
        public required Guid PublicationLatestPublishedReleaseVersionId { get; init; }    
    }
    
    public EventGridEvent ToEventGridEvent() => new(Subject, EventType, DataVersion, Payload);
}

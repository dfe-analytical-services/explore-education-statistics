using System;

namespace GovUk.Education.ExploreEducationStatistics.Publisher.Events;

public class ReleaseVersionPublishedEventDto
{
    // Changes to this event should also increment the version accordingly.
    public const string DataVersion = "1.0";
    
    public required Guid ReleaseVersionId { get; init; }
    public required Guid ReleaseId {get;init;}
    public required string ReleaseSlug { get; init; }
    public required Guid PublicationId { get; init; }
    public required string PublicationSlug { get; init; }
    public required Guid PublicationLatestReleaseVersionId { get; init; }
}

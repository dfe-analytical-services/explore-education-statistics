using System;

namespace GovUk.Education.ExploreEducationStatistics.Publisher.Services;

public record PublishedReleaseVersionInfo
{
    public Guid ReleaseVersionId { get; init; }
    public Guid ReleaseId { get; init; }
    public string ReleaseSlug { get; init; } = string.Empty;
    public Guid PublicationId { get; init; }
    public string PublicationSlug { get; init; } = string.Empty;
    public Guid PublicationLatestPublishedReleaseVersionId { get; init; }
}

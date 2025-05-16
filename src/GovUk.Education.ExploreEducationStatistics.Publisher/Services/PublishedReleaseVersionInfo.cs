using System;

namespace GovUk.Education.ExploreEducationStatistics.Publisher.Services;

public record PublishedReleaseVersionInfo
{
    public required Guid ReleaseVersionId { get; init; }

    public required Guid ReleaseId { get; init; }

    public required string ReleaseSlug { get; init; } = string.Empty;

    public required Guid PublicationId { get; init; }
}

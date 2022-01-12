#nullable enable
using System;

namespace GovUk.Education.ExploreEducationStatistics.Notifier.Model
{
    public record ReleaseNotificationMessage
    {
        public Guid PublicationId { get; init; }
        public string PublicationName { get; init; } = string.Empty;
        public string PublicationSlug { get; init; } = string.Empty;

        public string ReleaseName { get; init; } = string.Empty;
        public string ReleaseSlug { get; init; } = string.Empty;

        public bool Amendment { get; init; }
        public string UpdateNote { get; init; } = string.Empty;
    }
}

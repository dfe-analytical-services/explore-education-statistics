#nullable enable
using System;
using System.Collections.Generic;
using GovUk.Education.ExploreEducationStatistics.Common.ViewModels;

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

        public List<IdTitleViewModel> SupersededPublications { get; set; } = new();
    }
}

#nullable enable
using System;
using System.Collections.Generic;

namespace GovUk.Education.ExploreEducationStatistics.Content.Services.ViewModels
{
    public record PublicationViewModel
    {
        public Guid Id { get; set; }

        public string Title { get; set; } = string.Empty;

        public string Slug { get; set; } = string.Empty;

        public Guid LatestReleaseId { get; set; }

        public bool IsSuperseded { get; set; }

        public List<ReleaseTitleViewModel> Releases { get; set; } = new();

        public List<LegacyReleaseViewModel> LegacyReleases { get; set; } = new();

        public TopicViewModel Topic { get; set; }

        public ContactViewModel Contact { get; set; }

        public ExternalMethodologyViewModel ExternalMethodology { get; set; }

        public List<MethodologyVersionSummaryViewModel> Methodologies { get; set; } = new();
    }
}

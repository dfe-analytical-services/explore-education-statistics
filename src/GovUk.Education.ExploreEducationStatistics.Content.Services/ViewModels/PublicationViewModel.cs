using System;
using System.Collections.Generic;

namespace GovUk.Education.ExploreEducationStatistics.Content.Services.ViewModels
{
    public class PublicationViewModel
    {
        public Guid Id { get; set; }

        public string Title { get; set; }

        public string Slug { get; set; }

        public Guid LatestReleaseId { get; set; }

        public bool IsSuperseded { get; set; }

        public List<ReleaseTitleViewModel> Releases { get; set; }

        public List<LegacyReleaseViewModel> LegacyReleases { get; set; }

        public TopicViewModel Topic { get; set; }

        public ContactViewModel Contact { get; set; }

        public ExternalMethodologyViewModel ExternalMethodology { get; set; }

        public List<MethodologyVersionSummaryViewModel> Methodologies { get; set; }
    }
}

#nullable enable
using System;
using System.Collections.Generic;
using GovUk.Education.ExploreEducationStatistics.Publisher.Model.ViewModels;

namespace GovUk.Education.ExploreEducationStatistics.Content.Services.ViewModels
{
    public class CachedPublicationViewModel
    {
        public Guid Id { get; set; }

        public string Title { get; set; } = null!;

        public string Slug { get; set; } = null!;

        public Guid LatestReleaseId { get; set; }

        public List<ReleaseTitleViewModel> Releases { get; set; } = null!;

        public List<LegacyReleaseViewModel> LegacyReleases { get; set; } = null!;

        public TopicViewModel Topic { get; set; } = null!;

        public ContactViewModel Contact { get; set; } = null!;

        public ExternalMethodologyViewModel ExternalMethodology { get; set; } = null!;
    }
}

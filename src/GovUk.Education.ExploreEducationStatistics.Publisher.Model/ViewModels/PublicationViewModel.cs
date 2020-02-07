using System;
using System.Collections.Generic;

namespace GovUk.Education.ExploreEducationStatistics.Publisher.Model.ViewModels
{
    public class PublicationViewModel
    {
        public Guid Id { get; set; }

        public string Title { get; set; }

        public string Slug { get; set; }

        public string Description { get; set; }

        public string DataSource { get; set; }

        public string Summary { get; set; }

        public DateTime? NextUpdate { get; set; }
        
        public Guid LatestReleaseId { get; set; }

        public List<ReleaseTitleViewModel> Releases { get; set; }

        public List<LinkViewModel> LegacyReleases { get; set; }

        public TopicViewModel Topic { get; set; }

        public ContactViewModel Contact { get; set; }

        public MethodologySummaryViewModel Methodology { get; set; }
    }
}
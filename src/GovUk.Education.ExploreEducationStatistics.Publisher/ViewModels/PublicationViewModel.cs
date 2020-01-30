using System;
using System.Collections.Generic;
using GovUk.Education.ExploreEducationStatistics.Content.Model;

namespace GovUk.Education.ExploreEducationStatistics.Publisher.ViewModels
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
        
        public List<PreviousReleaseViewModel> Releases { get; set; }
        
        public List<BasicLink> LegacyReleases { get; set; }

        public TopicViewModel Topic { get; set; }
        
        public ContactViewModel Contact { get; set; }
        
        public MethodologyViewModel Methodology { get; set; }
    }
}
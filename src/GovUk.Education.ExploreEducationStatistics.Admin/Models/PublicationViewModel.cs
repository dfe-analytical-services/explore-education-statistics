using System;
using System.Collections.Generic;
using GovUk.Education.ExploreEducationStatistics.Content.Model;

namespace GovUk.Education.ExploreEducationStatistics.Admin.Models
{
    public class PublicationViewModel
    {
        public Guid Id { get; set; }
        
        public string Title { get; set; }

        public DateTime? NextUpdate { get; set; }

        public List<ReleaseViewModel> Releases { get; set; }
        
        public List<Methodology> Methodologies { get; set; }
        
        public Guid TopicId { get; set; }
        
        public Contact Contact { get; set; }
    }
}
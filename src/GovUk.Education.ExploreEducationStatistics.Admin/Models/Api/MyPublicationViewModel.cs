using System;
using System.Collections.Generic;
using GovUk.Education.ExploreEducationStatistics.Content.Model;

namespace GovUk.Education.ExploreEducationStatistics.Admin.Models.Api
{
    public class MyPublicationViewModel
    {
        public Guid Id { get; set; }
        
        public string Title { get; set; }

        public DateTime? NextUpdate { get; set; }

        public List<MyReleaseViewModel> Releases { get; set; }
        
        public MethodologyTitleViewModel Methodology { get; set; }

        public ExternalMethodology ExternalMethodology { get; set; }
        
        public Guid TopicId { get; set; }
        
        public Guid ThemeId { get; set; }
        
        public Contact Contact { get; set; }

        public PermissionsSet Permissions { get; set; }

        public class PermissionsSet
        {
            public bool CanUpdatePublication { get; set; }
            public bool CanCreateReleases { get; set; }
        }
    }
}
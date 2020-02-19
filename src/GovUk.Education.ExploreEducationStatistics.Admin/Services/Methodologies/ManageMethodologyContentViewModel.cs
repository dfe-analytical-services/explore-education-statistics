using System;
using System.Collections.Generic;
using GovUk.Education.ExploreEducationStatistics.Admin.Models.Api;

namespace GovUk.Education.ExploreEducationStatistics.Admin.Services.Methodologies
{
    public class ManageMethodologyContentViewModel
    {
        public Guid Id { get; set; }

        public string Title { get; set; }
        
        public string Status { get; set; }
        
        public List<ContentSectionViewModel> Content { get; set; }
        
        public List<ContentSectionViewModel> Annexes { get; set; }
    }
}
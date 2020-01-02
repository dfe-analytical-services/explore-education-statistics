using System;
using System.Collections.Generic;
using GovUk.Education.ExploreEducationStatistics.Data.Model;

namespace GovUk.Education.ExploreEducationStatistics.Admin.Models.Api
{
    public class MethodologyStatusViewModel
    {
        public Guid Id { get; set; }

        public string Title { get; set; }
        
        public string Status { get; set; }

        public List<MethodologyStatusPublications> Publications { get; set; }
    }

    public class MethodologyStatusPublications
    {
        public Guid Id { get; set; }
        
        public string Title { get; set; }
    }
}
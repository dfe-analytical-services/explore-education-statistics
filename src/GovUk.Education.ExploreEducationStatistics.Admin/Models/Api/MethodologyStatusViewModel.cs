using System;
using System.Collections.Generic;
using GovUk.Education.ExploreEducationStatistics.Data.Model;

namespace GovUk.Education.ExploreEducationStatistics.Admin.Models.Api
{
    public class MethodologyStatusViewModel
    {
        public Guid Id { get; set; }

        public string Title { get; set; }
        
        // TODO: fake the status until the it is added to methodology as a concept
        public string Status => "Live";

        public List<MethodologyStatusPublications> Publications { get; set; }
    }

    public class MethodologyStatusPublications
    {
        public Guid Id { get; set; }
        
        public string Title { get; set; }
    }
}
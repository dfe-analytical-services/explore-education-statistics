using System;
using System.Collections.Generic;
namespace GovUk.Education.ExploreEducationStatistics.Admin.Models
{
    public class Theme
    {
        
        
        public string Title { get; set; }
        public Guid Id { get; set; }

        public List<Topic> Topics { get; set; }
    }
}
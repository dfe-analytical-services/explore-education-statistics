using System;
using System.ComponentModel.DataAnnotations;

namespace GovUk.Education.ExploreEducationStatistics.Content.Api.Models
{
    public class ContentSection
    {
        public int Order { get; set; }
        
        public string Heading { get; set; }
        
        public string Caption { get; set; }
    }
}
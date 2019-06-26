using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace GovUk.Education.ExploreEducationStatistics.Content.Model
{
    public class Methodology
    {
        [Key] [Required] public Guid Id { get; set; }

        [Required] public string Title { get; set; }
        
        public string Summary { get; set; }
        
        public DateTime? Published { get; set; }

        public DateTime? LastUpdated { get; set; }
 
        public List<ContentSection> Content { get; set; }
        
        public List<ContentSection> Annexes { get; set; }
        
        public Guid PublicationId { get; set; }

        public Publication Publication { get; set; }
    }
}
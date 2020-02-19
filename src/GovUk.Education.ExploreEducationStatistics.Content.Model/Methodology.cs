using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.Azure.Documents.SystemFunctions;

namespace GovUk.Education.ExploreEducationStatistics.Content.Model
{
    public class Methodology
    {
        [Key] 
        [Required] 
        public Guid Id { get; set; }

        [Required]
        public string Title { get; set; }

        public string Slug { get; set; }

        public string Summary { get; set; }

        // TODO EES-1315 - represent Methodology status with enum
        [NotMapped]
        public string Status => Published == null ?  "Draft" : "Live" ;

        public DateTime? Published { get; set; }
        
        public DateTime? PublishScheduled { get; set; }

        public DateTime? LastUpdated { get; set; }
 
        public List<ContentSection> Content { get; set; }
        
        public List<ContentSection> Annexes { get; set; }

        public List<Publication> Publications { get; set; }
    }
}
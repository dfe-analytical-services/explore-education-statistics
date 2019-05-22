using System;
using System.ComponentModel.DataAnnotations;

namespace GovUk.Education.ExploreEducationStatistics.Content.Model
{
    public class Update
    {
        [Key] [Required] public Guid Id { get; set; }

        public Guid ReleaseId { get; set; }
        
        public Release Release { get; set; }

        [Required] public DateTime On { get; set; }

        [Required] public string Reason { get; set; }
    }
}
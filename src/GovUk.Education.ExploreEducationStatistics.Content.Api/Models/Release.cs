using System;
using System.ComponentModel.DataAnnotations;

namespace GovUk.Education.ExploreEducationStatistics.Content.Api.Models
{
    public class Release
    {
        [Key]
        [Required]
        public Guid Id { get; set; }

        [Required]
        public string Title { get; set; }
        
        public string ReleaseName { get; set; }
        
        public DateTime? Published { get; set; }
        
        public string Summary { get; set; }
        
        public Guid PublicationId { get; set; }
    }
}
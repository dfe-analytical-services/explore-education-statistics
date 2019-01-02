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

        public Guid PublicationId { get; set; }
        
        public Publication Publication { get; set; }
    }
}
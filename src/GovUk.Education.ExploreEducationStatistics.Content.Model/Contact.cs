using System;
using System.ComponentModel.DataAnnotations;

namespace GovUk.Education.ExploreEducationStatistics.Content.Model
{
    public class Contact
    {
        [Key] [Required] public Guid Id { get; set; }

        [Required]
        public string TeamName { get; set; }

        [Required]
        [EmailAddress]
        public string TeamEmail { get; set; }

        [Required]
        public string ContactName { get; set; }

        [Required]
        public string ContactTelNo { get; set; }
    }
}
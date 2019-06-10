using System;
using System.ComponentModel.DataAnnotations;

namespace GovUk.Education.ExploreEducationStatistics.Content.Model
{
    public class Contact
    {
        [Key] [Required] public Guid Id { get; set; }

        public string TeamName { get; set; }

        public string TeamEmail { get; set; }

        public string ContactName { get; set; }

        public string ContactTelNo { get; set; }
    }
}
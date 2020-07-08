using System.ComponentModel.DataAnnotations;
using System;

namespace GovUk.Education.ExploreEducationStatistics.Content.Model
{
    public class ExternalMethodology
    {
        [Required]
        public string Title { get; set; }

        [Required]
        [Url]
        public Uri Url { get; set; }
    }
}
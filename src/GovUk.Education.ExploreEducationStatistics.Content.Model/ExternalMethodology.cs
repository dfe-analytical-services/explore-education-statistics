using System.ComponentModel.DataAnnotations;

namespace GovUk.Education.ExploreEducationStatistics.Content.Model
{
    public class ExternalMethodology
    {
        [Required]
        public string Title { get; set; }

        [Required]
        [Url]
        public string Url { get; set; }
    }
}

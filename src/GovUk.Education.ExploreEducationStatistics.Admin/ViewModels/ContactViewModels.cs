using System.ComponentModel.DataAnnotations;

namespace GovUk.Education.ExploreEducationStatistics.Admin.ViewModels
{
    public class ContactViewModel
    {
        public string TeamName { get; set; }

        public string TeamEmail { get; set; }

        public string ContactName { get; set; }

        public string ContactTelNo { get; set; }
    }

    public class ContactSaveViewModel
    {
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

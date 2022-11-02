#nullable enable
using System.ComponentModel.DataAnnotations;

namespace GovUk.Education.ExploreEducationStatistics.Admin.ViewModels
{
    public class ContactViewModel
    {
        public string TeamName { get; set; } = string.Empty;

        public string TeamEmail { get; set; } = string.Empty;

        public string ContactName { get; set; } = string.Empty;

        public string ContactTelNo { get; set; } = string.Empty;
    }

    public class ContactSaveViewModel
    {
        [Required] public string TeamName { get; set; } = string.Empty;

        [Required, EmailAddress] public string TeamEmail { get; set; } = string.Empty;

        [Required] public string ContactName { get; set; } = string.Empty;

        [Required] public string ContactTelNo { get; set; } = string.Empty;
    }
}

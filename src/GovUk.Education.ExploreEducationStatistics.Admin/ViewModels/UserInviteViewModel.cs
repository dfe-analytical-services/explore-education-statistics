using System.ComponentModel.DataAnnotations;

#nullable enable
namespace GovUk.Education.ExploreEducationStatistics.Admin.ViewModels
{
    public class UserInviteViewModel
    {
        [EmailAddress] public string Email { get; set; } = string.Empty;

        [Required(AllowEmptyStrings = false)]
        public string RoleId { get; set; } = string.Empty;
    }
}

#nullable enable
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace GovUk.Education.ExploreEducationStatistics.Admin.ViewModels
{
    public class UserInviteViewModel
    {
        [EmailAddress] public string Email { get; set; } = string.Empty;

        [Required(AllowEmptyStrings = false)]
        public string RoleId { get; set; } = string.Empty;

        public List<UserReleaseRoleAddViewModel> UserReleaseRoles { get; set; } = new();

        public List<UserPublicationRoleAddViewModel> UserPublicationRoles { get; set; } = new();
    }
}

#nullable enable
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace GovUk.Education.ExploreEducationStatistics.Admin.ViewModels
{
    public class UserInviteCreateRequest
    {
        [EmailAddress] public string Email { get; set; } = string.Empty;

        [Required(AllowEmptyStrings = false)]
        public string RoleId { get; set; } = string.Empty;

        public DateTime? CreatedDate { get; set; } = null;

        public List<UserReleaseRoleCreateRequest> UserReleaseRoles { get; set; } = new();

        public List<UserPublicationRoleCreateRequest> UserPublicationRoles { get; set; } = new();
    }
}

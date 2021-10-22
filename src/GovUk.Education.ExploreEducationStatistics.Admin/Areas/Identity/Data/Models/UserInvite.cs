#nullable enable
using System;
using System.ComponentModel.DataAnnotations;
using GovUk.Education.ExploreEducationStatistics.Admin.Models;
using Microsoft.AspNetCore.Identity;

namespace GovUk.Education.ExploreEducationStatistics.Admin.Areas.Identity.Data.Models
{
    public class UserInvite
    {
        [Key] public string Email { get; set; } = string.Empty;

        public bool Accepted { get; set; }

        public IdentityRole Role { get; set; } = null!;

        public string RoleId { get; set; } = string.Empty;

        public DateTime Created { get; set; }

        public ApplicationUser? CreatedBy { get; set; }

        public string? CreatedById { get; set; }
    }
}

#nullable enable
using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using GovUk.Education.ExploreEducationStatistics.Admin.Models;
using Microsoft.AspNetCore.Identity;

namespace GovUk.Education.ExploreEducationStatistics.Admin.Areas.Identity.Data.Models
{
    public class UserInvite
    {
        public static int InviteExpiryDurationDays = 14;
        
        [Key] public string Email { get; set; } = string.Empty;

        public bool Accepted { get; set; }

        public IdentityRole Role { get; set; } = null!;

        public string RoleId { get; set; } = string.Empty;

        public DateTime Created { get; set; }

        public ApplicationUser? CreatedBy { get; set; }

        public string? CreatedById { get; set; }

        /// <remarks>
        /// Note that this logic is also present in <see cref="UsersAndRolesDbContext.OnModelCreating"/>.
        /// It is implemented there as well as EF is not able to translate this computed field for use in
        /// a QueryFilter. 
        /// </remarks>
        [NotMapped] public bool Expired => !Accepted && 
                                           Created < DateTime.UtcNow.AddDays(-InviteExpiryDurationDays);
    }
}

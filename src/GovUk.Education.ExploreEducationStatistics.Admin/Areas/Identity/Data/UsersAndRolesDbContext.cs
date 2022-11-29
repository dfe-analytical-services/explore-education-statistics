#nullable enable
using System;
using GovUk.Education.ExploreEducationStatistics.Admin.Areas.Identity.Data.Models;
using GovUk.Education.ExploreEducationStatistics.Admin.Models;
using GovUk.Education.ExploreEducationStatistics.Admin.Security;
using GovUk.Education.ExploreEducationStatistics.Common.Extensions;
using GovUk.Education.ExploreEducationStatistics.Common.Utils;
using IdentityServer4.EntityFramework.Options;
using Microsoft.AspNetCore.ApiAuthorization.IdentityServer;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using static GovUk.Education.ExploreEducationStatistics.Admin.Models.GlobalRoles;

namespace GovUk.Education.ExploreEducationStatistics.Admin.Areas.Identity.Data
{
    public class UsersAndRolesDbContext : ApiAuthorizationDbContext<ApplicationUser>
    {
        public DbSet<UserInvite> UserInvites { get; set; } = null!;

        public UsersAndRolesDbContext(
            DbContextOptions<UsersAndRolesDbContext> options,
            IOptions<OperationalStoreOptions> operationalStoreOptions,
            bool updateTimestamps = true) : base(options, operationalStoreOptions)
        {
            Configure(updateTimestamps);
        }

        private void Configure(bool updateTimestamps = true)
        {
            if (updateTimestamps)
            {
                ChangeTracker.StateChanged += DbContextUtils.UpdateTimestamps;
                ChangeTracker.Tracked += DbContextUtils.UpdateTimestamps;
            }
        }

        private static IdentityRole<string> CreateRole(Role role)
        {
            return new IdentityRole<string>
            {
                Id = role.GetEnumValue(),
                Name = role.GetEnumLabel(),
                NormalizedName = role.GetEnumLabel().ToUpper(),
                ConcurrencyStamp = "85d6c75e-a6c8-4c7e-b4d0-8ee70a4879d3",
            };
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<UserInvite>()
                .Property(invite => invite.Created)
                .HasConversion(
                    v => v,
                    v => DateTime.SpecifyKind(v, DateTimeKind.Utc));

            // Note that this logic is also present in UserInvite.Expired.
            // It is implemented here as well because EF is not able to translate the "Expired" computed field for
            // use in this QueryFilter. 
            modelBuilder.Entity<UserInvite>()
                .HasQueryFilter(invite => invite.Accepted || 
                                          invite.Created >= DateTime.UtcNow.AddDays(-UserInvite.InviteExpiryDurationDays));

            modelBuilder.Entity<IdentityRole>()
                .HasData(
                    CreateRole(Role.BauUser),
                    CreateRole(Role.Analyst),
                    CreateRole(Role.PrereleaseUser)
                );

            string bauRoleId = Role.BauUser.GetEnumValue();
            string analystRoleId = Role.Analyst.GetEnumValue();
            string prereleaseRoleId = Role.PrereleaseUser.GetEnumValue();

            modelBuilder.Entity<IdentityRoleClaim<string>>()
                .HasData(
                    new IdentityRoleClaim<string>
                    {
                        Id = -2,
                        RoleId = bauRoleId,
                        ClaimType = SecurityClaimTypes.ApplicationAccessGranted.ToString(),
                        ClaimValue = "",
                    },
                    new IdentityRoleClaim<string>
                    {
                        Id = -3,
                        RoleId = bauRoleId,
                        ClaimType = SecurityClaimTypes.AccessAllReleases.ToString(),
                        ClaimValue = "",
                    },
                    new IdentityRoleClaim<string>
                    {
                        Id = -5,
                        RoleId = bauRoleId,
                        ClaimType = SecurityClaimTypes.MarkAllReleasesAsDraft.ToString(),
                        ClaimValue = "",
                    },
                    new IdentityRoleClaim<string>
                    {
                        Id = -6,
                        RoleId = bauRoleId,
                        ClaimType = SecurityClaimTypes.SubmitAllReleasesToHigherReview.ToString(),
                        ClaimValue = "",
                    },
                    new IdentityRoleClaim<string>
                    {
                        Id = -7,
                        RoleId = bauRoleId,
                        ClaimType = SecurityClaimTypes.ApproveAllReleases.ToString(),
                        ClaimValue = "",
                    },
                    new IdentityRoleClaim<string>
                    {
                        Id = -8,
                        RoleId = bauRoleId,
                        ClaimType = SecurityClaimTypes.UpdateAllReleases.ToString(),
                        ClaimValue = "",
                    },
                    new IdentityRoleClaim<string>
                    {
                        Id = -9,
                        RoleId = bauRoleId,
                        ClaimType = SecurityClaimTypes.CreateAnyPublication.ToString(),
                        ClaimValue = "",
                    },
                    new IdentityRoleClaim<string>
                    {
                        Id = -10,
                        RoleId = bauRoleId,
                        ClaimType = SecurityClaimTypes.CreateAnyRelease.ToString(),
                        ClaimValue = "",
                    },
                    new IdentityRoleClaim<string>
                    {
                        Id = -11,
                        RoleId = bauRoleId,
                        ClaimType = SecurityClaimTypes.ManageAnyUser.ToString(),
                        ClaimValue = "",
                    },
                    new IdentityRoleClaim<string>
                    {
                        Id = -13,
                        RoleId = analystRoleId,
                        ClaimType = SecurityClaimTypes.ApplicationAccessGranted.ToString(),
                        ClaimValue = "",
                    },
                    new IdentityRoleClaim<string>
                    {
                        Id = -14,
                        RoleId = analystRoleId,
                        ClaimType = SecurityClaimTypes.AnalystPagesAccessGranted.ToString(),
                        ClaimValue = "",
                    },
                    new IdentityRoleClaim<string>
                    {
                        Id = -15,
                        RoleId = prereleaseRoleId,
                        ClaimType = SecurityClaimTypes.ApplicationAccessGranted.ToString(),
                        ClaimValue = "",
                    },
                    new IdentityRoleClaim<string>
                    {
                        Id = -16,
                        RoleId = prereleaseRoleId,
                        ClaimType = SecurityClaimTypes.PrereleasePagesAccessGranted.ToString(),
                        ClaimValue = "",
                    },
                    new IdentityRoleClaim<string>
                    {
                        Id = -17,
                        RoleId = analystRoleId,
                        ClaimType = SecurityClaimTypes.PrereleasePagesAccessGranted.ToString(),
                        ClaimValue = "",
                    },
                    new IdentityRoleClaim<string>
                    {
                        Id = -18,
                        RoleId = bauRoleId,
                        ClaimType = SecurityClaimTypes.AnalystPagesAccessGranted.ToString(),
                        ClaimValue = "",
                    },
                    new IdentityRoleClaim<string>
                    {
                        Id = -19,
                        RoleId = bauRoleId,
                        ClaimType = SecurityClaimTypes.PrereleasePagesAccessGranted.ToString(),
                        ClaimValue = "",
                    },
                    new IdentityRoleClaim<string>
                    {
                        Id = -20,
                        RoleId = bauRoleId,
                        ClaimType = SecurityClaimTypes.CanViewPrereleaseContacts.ToString(),
                        ClaimValue = "",
                    },
                    new IdentityRoleClaim<string>
                    {
                        Id = -21,
                        RoleId = analystRoleId,
                        ClaimType = SecurityClaimTypes.CanViewPrereleaseContacts.ToString(),
                        ClaimValue = "",
                    },
                    new IdentityRoleClaim<string>
                    {
                        Id = -22,
                        RoleId = bauRoleId,
                        ClaimType = SecurityClaimTypes.CreateAnyMethodology.ToString(),
                        ClaimValue = "",
                    },
                    new IdentityRoleClaim<string>
                    {
                        Id = -23,
                        RoleId = bauRoleId,
                        ClaimType = SecurityClaimTypes.UpdateAllMethodologies.ToString(),
                        ClaimValue = "",
                    },
                    new IdentityRoleClaim<string>
                    {
                        Id = -24,
                        RoleId = bauRoleId,
                        ClaimType = SecurityClaimTypes.AccessAllMethodologies.ToString(),
                        ClaimValue = "",
                    },
                    new IdentityRoleClaim<string>
                    {
                        Id = -29,
                        RoleId = bauRoleId,
                        ClaimType = SecurityClaimTypes.ApproveAllMethodologies.ToString(),
                        ClaimValue = "",
                    },
                    new IdentityRoleClaim<string>
                    {
                        Id = -30,
                        RoleId = bauRoleId,
                        ClaimType = SecurityClaimTypes.PublishAllReleases.ToString(),
                        ClaimValue = "",
                    },
                    new IdentityRoleClaim<string>
                    {
                        Id = -31,
                        RoleId = bauRoleId,
                        ClaimType = SecurityClaimTypes.MakeAmendmentsOfAllReleases.ToString(),
                        ClaimValue = "",
                    },
                    new IdentityRoleClaim<string>
                    {
                        Id = -32,
                        RoleId = bauRoleId,
                        ClaimType = SecurityClaimTypes.DeleteAllReleaseAmendments.ToString(),
                        ClaimValue = "",
                    },
                    new IdentityRoleClaim<string>
                    {
                        Id = -33,
                        RoleId = bauRoleId,
                        ClaimType = SecurityClaimTypes.ManageAllTaxonomy.ToString(),
                        ClaimValue = "",
                    },
                    new IdentityRoleClaim<string>
                    {
                        Id = -34,
                        RoleId = bauRoleId,
                        ClaimType = SecurityClaimTypes.UpdateAllPublications.ToString(),
                        ClaimValue = "",
                    },
                    new IdentityRoleClaim<string>
                    {
                        Id = -35,
                        RoleId = bauRoleId,
                        ClaimType = SecurityClaimTypes.MarkAllMethodologiesDraft.ToString(),
                        ClaimValue = "",
                    },
                    new IdentityRoleClaim<string>
                    {
                        Id = -36,
                        RoleId = bauRoleId,
                        ClaimType = SecurityClaimTypes.AccessAllImports.ToString(),
                        ClaimValue = "",
                    },
                    new
                    {
                        Id = -37,
                        RoleId = bauRoleId,
                        ClaimType = SecurityClaimTypes.CancelAllFileImports.ToString(),
                        ClaimValue = "",
                    },
                    new
                    {
                        Id = -38,
                        RoleId = bauRoleId,
                        ClaimType = SecurityClaimTypes.MakeAmendmentsOfAllMethodologies.ToString(),
                        ClaimValue = "",
                    },
                    new
                    {
                        Id = -39,
                        RoleId = bauRoleId,
                        ClaimType = SecurityClaimTypes.DeleteAllMethodologies.ToString(),
                        ClaimValue = "",
                    },
                    new IdentityRoleClaim<string>
                    {
                        Id = -40,
                        RoleId = bauRoleId,
                        ClaimType = SecurityClaimTypes.AdoptAnyMethodology.ToString(),
                        ClaimValue = "",
                    },
                    new IdentityRoleClaim<string>
                    {
                        Id = -41,
                        RoleId = bauRoleId,
                        ClaimType = SecurityClaimTypes.AccessAllPublications.ToString(),
                        ClaimValue = "",
                    }
                );
        }
    }
}

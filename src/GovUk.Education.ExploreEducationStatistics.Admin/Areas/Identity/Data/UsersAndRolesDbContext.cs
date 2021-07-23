using GovUk.Education.ExploreEducationStatistics.Admin.Areas.Identity.Data.Models;
using GovUk.Education.ExploreEducationStatistics.Admin.Models;
using GovUk.Education.ExploreEducationStatistics.Admin.Security;
using IdentityServer4.EntityFramework.Options;
using Microsoft.AspNetCore.ApiAuthorization.IdentityServer;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

namespace GovUk.Education.ExploreEducationStatistics.Admin.Areas.Identity.Data
{
    public class UsersAndRolesDbContext : ApiAuthorizationDbContext<ApplicationUser>
    {
        public DbSet<UserInvite> UserInvites { get; set; }

        public UsersAndRolesDbContext(
            DbContextOptions<UsersAndRolesDbContext> options,
            IOptions<OperationalStoreOptions> operationalStoreOptions) : base(options, operationalStoreOptions)
        {
        }

        private static IdentityRole<string> CreateRole(
            string id,
            string name
        )
        {
            return new IdentityRole<string>
            {
                Id = id,
                Name = name,
                NormalizedName = name.ToUpper(),
                ConcurrencyStamp = "85d6c75e-a6c8-4c7e-b4d0-8ee70a4879d3",
            };
        }
        
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
         
            const string bauUserRoleId = "cf67b697-bddd-41bd-86e0-11b7e11d99b3";
            const string analystRoleId = "f9ddb43e-aa9e-41ed-837d-3062e130c425";
            const string prereleaseUserRoleId = "17e634f4-7a2b-4a23-8636-b079877b4232";

            modelBuilder.Entity<IdentityRole>()
                .HasData(
                    CreateRole(
                        bauUserRoleId,
                        "BAU User"
                    ),
                    CreateRole(
                        analystRoleId,
                        "Analyst"
                    ),
                    CreateRole(
                        prereleaseUserRoleId,
                        "Prerelease User"
                    )
                );

            modelBuilder.Entity<IdentityRoleClaim<string>>()
                .HasData(
                    new IdentityRoleClaim<string>
                    {
                        Id = -2,
                        RoleId = bauUserRoleId,
                        ClaimType = SecurityClaimTypes.ApplicationAccessGranted.ToString(),
                        ClaimValue = "",
                    },
                    new IdentityRoleClaim<string>
                    {
                        Id = -3,
                        RoleId = bauUserRoleId,
                        ClaimType = SecurityClaimTypes.AccessAllReleases.ToString(),
                        ClaimValue = "",
                    },
                    new IdentityRoleClaim<string>
                    {
                        Id = -5,
                        RoleId = bauUserRoleId,
                        ClaimType = SecurityClaimTypes.MarkAllReleasesAsDraft.ToString(),
                        ClaimValue = "",
                    },
                    new IdentityRoleClaim<string>
                    {
                        Id = -6,
                        RoleId = bauUserRoleId,
                        ClaimType = SecurityClaimTypes.SubmitAllReleasesToHigherReview.ToString(),
                        ClaimValue = "",
                    },
                    new IdentityRoleClaim<string>
                    {
                        Id = -7,
                        RoleId = bauUserRoleId,
                        ClaimType = SecurityClaimTypes.ApproveAllReleases.ToString(),
                        ClaimValue = "",
                    },
                    new IdentityRoleClaim<string>
                    {
                        Id = -8,
                        RoleId = bauUserRoleId,
                        ClaimType = SecurityClaimTypes.UpdateAllReleases.ToString(),
                        ClaimValue = "",
                    },
                    new IdentityRoleClaim<string>
                    {
                        Id = -9,
                        RoleId = bauUserRoleId,
                        ClaimType = SecurityClaimTypes.CreateAnyPublication.ToString(),
                        ClaimValue = "",
                    },
                    new IdentityRoleClaim<string>
                    {
                        Id = -10,
                        RoleId = bauUserRoleId,
                        ClaimType = SecurityClaimTypes.CreateAnyRelease.ToString(),
                        ClaimValue = "",
                    },
                    new IdentityRoleClaim<string>
                    {
                        Id = -11,
                        RoleId = bauUserRoleId,
                        ClaimType = SecurityClaimTypes.ManageAnyUser.ToString(),
                        ClaimValue = "",
                    },
                    new IdentityRoleClaim<string>
                    {
                        Id = -12,
                        RoleId = bauUserRoleId,
                        ClaimType = SecurityClaimTypes.ManageAnyMethodology.ToString(),
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
                        RoleId = prereleaseUserRoleId,
                        ClaimType = SecurityClaimTypes.ApplicationAccessGranted.ToString(),
                        ClaimValue = "",
                    },
                    new IdentityRoleClaim<string>
                    {
                        Id = -16,
                        RoleId = prereleaseUserRoleId,
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
                        RoleId = bauUserRoleId,
                        ClaimType = SecurityClaimTypes.AnalystPagesAccessGranted.ToString(),
                        ClaimValue = "",
                    },
                    new IdentityRoleClaim<string>
                    {
                        Id = -19,
                        RoleId = bauUserRoleId,
                        ClaimType = SecurityClaimTypes.PrereleasePagesAccessGranted.ToString(),
                        ClaimValue = "",
                    },
                    new IdentityRoleClaim<string>
                    {
                        Id = -20,
                        RoleId = bauUserRoleId,
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
                        RoleId = bauUserRoleId,
                        ClaimType = SecurityClaimTypes.CreateAnyMethodology.ToString(),
                        ClaimValue = "",
                    },
                    new IdentityRoleClaim<string>
                    {
                        Id = -23,
                        RoleId = bauUserRoleId,
                        ClaimType = SecurityClaimTypes.UpdateAllMethodologies.ToString(),
                        ClaimValue = "",
                    },
                    new IdentityRoleClaim<string>
                    {
                        Id = -24,
                        RoleId = bauUserRoleId,
                        ClaimType = SecurityClaimTypes.AccessAllMethodologies.ToString(),
                        ClaimValue = "",
                    },
                    // TODO SOW4 EES-2168 Drop claim UpdateAllMethodologies from the Analyst role
                    new IdentityRoleClaim<string>
                    {
                        Id = -26,
                        RoleId = analystRoleId,
                        ClaimType = SecurityClaimTypes.UpdateAllMethodologies.ToString(),
                        ClaimValue = "",
                    },
                    new IdentityRoleClaim<string>
                    {
                        Id = -27,
                        RoleId = analystRoleId,
                        ClaimType = SecurityClaimTypes.AccessAllMethodologies.ToString(),
                        ClaimValue = "",
                    },
                    new IdentityRoleClaim<string>
                    {
                        Id = -28,
                        RoleId = analystRoleId,
                        ClaimType = SecurityClaimTypes.ApproveAllMethodologies.ToString(),
                        ClaimValue = "",
                    },
                    new IdentityRoleClaim<string>
                    {
                        Id = -29,
                        RoleId = bauUserRoleId,
                        ClaimType = SecurityClaimTypes.ApproveAllMethodologies.ToString(),
                        ClaimValue = "",
                    },
                    new IdentityRoleClaim<string>
                    {
                        Id = -30,
                        RoleId = bauUserRoleId,
                        ClaimType = SecurityClaimTypes.PublishAllReleases.ToString(),
                        ClaimValue = "",
                    },
                    new IdentityRoleClaim<string>
                    {
                        Id = -31,
                        RoleId = bauUserRoleId,
                        ClaimType = SecurityClaimTypes.MakeAmendmentsOfAllReleases.ToString(),
                        ClaimValue = "",
                    },
                    new IdentityRoleClaim<string>
                    {
                        Id = -32,
                        RoleId = bauUserRoleId,
                        ClaimType = SecurityClaimTypes.DeleteAllReleaseAmendments.ToString(),
                        ClaimValue = "",
                    },
                    new IdentityRoleClaim<string>
                    {
                        Id = -33,
                        RoleId = bauUserRoleId,
                        ClaimType = SecurityClaimTypes.ManageAllTaxonomy.ToString(),
                        ClaimValue = "",
                    },
                    new IdentityRoleClaim<string>
                    {
                        Id = -34,
                        RoleId = bauUserRoleId,
                        ClaimType = SecurityClaimTypes.UpdateAllPublications.ToString(),
                        ClaimValue = "",
                    },
                    new IdentityRoleClaim<string>
                    {
                        Id = -35,
                        RoleId = bauUserRoleId,
                        ClaimType = SecurityClaimTypes.MarkAllMethodologiesDraft.ToString(),
                        ClaimValue = "",
                    },
                    new IdentityRoleClaim<string>
                    {
                        Id = -36,
                        RoleId = bauUserRoleId,
                        ClaimType = SecurityClaimTypes.AccessAllImports.ToString(),
                        ClaimValue = "",
                    },
                    new
                    {
                        Id = -37,
                        RoleId = bauUserRoleId,
                        ClaimType = SecurityClaimTypes.CancelAllFileImports.ToString(),
                        ClaimValue = "",
                    },
                    new
                    {
                        Id = -38,
                        RoleId = bauUserRoleId,
                        ClaimType = SecurityClaimTypes.MakeAmendmentsOfAllMethodologies.ToString(),
                        ClaimValue = "",
                    },
                    new
                    {
                        Id = -39,
                        RoleId = bauUserRoleId,
                        ClaimType = SecurityClaimTypes.DeleteAllMethodologies.ToString(),
                        ClaimValue = "",
                    }
                );
        }
    }
}

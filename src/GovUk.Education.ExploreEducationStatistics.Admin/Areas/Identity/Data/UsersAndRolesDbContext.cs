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

        private static ApplicationUser CreateApplicationUser(
            string id,
            string email,
            string firstName,
            string lastName
        )
        {
            return new ApplicationUser
            {
                Id = id,
                UserName = email,
                NormalizedUserName = email.ToUpper(),
                Email = email,
                NormalizedEmail = email.ToUpper(),
                FirstName = firstName,
                LastName = lastName,
                EmailConfirmed = false,
                ConcurrencyStamp = "85d6c75e-a6c8-4c7e-b4d0-8ee70a4879d3",
                SecurityStamp = "V7ZOUEOGN2HGZDN3HKPNIHLSUWWUHTA6",
                PasswordHash = null,
                PhoneNumber = null,
                PhoneNumberConfirmed = false,
                TwoFactorEnabled = false,
                LockoutEnd = null,
                LockoutEnabled = true,
                AccessFailedCount = 0
            };
        }

        private static IdentityUserLogin<string> CreateUserLogin(
            string userId,
            string providerKey
        )
        {
            return new IdentityUserLogin<string>
            {
                LoginProvider = "OpenIdConnect",
                ProviderKey = providerKey,
                UserId = userId,
                ProviderDisplayName = "OpenIdConnect"
            };
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
                        Id = -4,
                        RoleId = bauUserRoleId,
                        ClaimType = SecurityClaimTypes.AccessAllTopics.ToString(),
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
                    new IdentityRoleClaim<string>
                    {
                        Id = -25,
                        RoleId = analystRoleId,
                        ClaimType = SecurityClaimTypes.CreateAnyMethodology.ToString(),
                        ClaimValue = "",
                    },
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
                    }
                );
            
            const string analystUser1Id = "e7f7c82e-aaf3-43db-a5ab-755678f67d04";
            const string analystUser2Id = "6620bccf-2433-495e-995d-fc76c59d9c62";
            const string analystUser3Id = "b390b405-ef90-4b9d-8770-22948e53189a";

            const string bauUser1Id = "b99e8358-9a5e-4a3a-9288-6f94c7e1e3dd";
            const string bauUser2Id = "b6f0dfa5-0102-4b91-9aa8-f23b7d8aca63";

            const string prereleaseUser1Id = "d5c85378-df85-482c-a1ce-09654dae567d";
            const string prereleaseUser2Id = "ee9a02c1-b3f9-402c-9e9b-4fb78d737050";
            
            modelBuilder.Entity<ApplicationUser>()
                .HasData(
                    CreateApplicationUser(
                        analystUser1Id,
                        "analyst1@example.com",
                        "Analyst1",
                        "User1"
                    ),
                    CreateApplicationUser(
                        analystUser2Id,
                        "analyst2@example.com",
                        "Analyst2",
                        "User2"
                    ),
                    CreateApplicationUser(
                        analystUser3Id,
                        "analyst3@example.com",
                        "Analyst3",
                        "User3"
                    ),
                    CreateApplicationUser(
                        bauUser1Id,
                        "bau1@example.com",
                        "Bau1",
                        "User1"
                    ),
                    CreateApplicationUser(
                        bauUser2Id,
                        "bau2@example.com",
                        "Bau2",
                        "User2"
                    ),
                    CreateApplicationUser(
                        prereleaseUser1Id,
                        "prerelease1@example.com",
                        "Prerelease1",
                        "User1"
                    ),
                    CreateApplicationUser(
                        prereleaseUser2Id,
                        "prerelease2@example.com",
                        "Prerelease2",
                        "User2"
                    )
                );

            modelBuilder.Entity<IdentityUserRole<string>>()
                .HasData(
                    new IdentityUserRole<string>
                    {
                        UserId = analystUser1Id,
                        RoleId = analystRoleId
                    },
                    new IdentityUserRole<string>
                    {
                        UserId = analystUser2Id,
                        RoleId = analystRoleId
                    },
                    new IdentityUserRole<string>
                    {
                        UserId = analystUser3Id,
                        RoleId = analystRoleId
                    },
                    new IdentityUserRole<string>
                    {
                        UserId = bauUser1Id,
                        RoleId = bauUserRoleId
                    },
                    new IdentityUserRole<string>
                    {
                        UserId = bauUser2Id,
                        RoleId = bauUserRoleId
                    },
                    new IdentityUserRole<string>
                    {
                        UserId = prereleaseUser1Id,
                        RoleId = prereleaseUserRoleId
                    },
                    new IdentityUserRole<string>
                    {
                        UserId = prereleaseUser2Id,
                        RoleId = prereleaseUserRoleId
                    }
                );

            modelBuilder.Entity<IdentityUserLogin<string>>()
                .HasData(
                    CreateUserLogin(
                        analystUser1Id,
                        "5zzTEeAYz71aVPJ1ho1VGW3cYk7_qcQpkDqYYxbH3po"
                    ),
                    CreateUserLogin(
                        analystUser2Id,
                        "RLdgJMsfN6QVjpCbkaOYIpzh6DA3QpRfnBcfIx46uDM"
                    ),
                    CreateUserLogin(
                        analystUser3Id,
                        "ces_f2I3zCjGZ9HUprWF3RiQgswrKvPFAY1Lwu_KI6M"
                    ),
                    CreateUserLogin(
                        bauUser1Id,
                        "cb3XrjF6BLuMZ5P3aRo8wBobF7tAshdk2gF0X5Qm68o"
                    ),                    
                    CreateUserLogin(
                        bauUser2Id,
                        "EKTK7hPGgxGVxRSBjgTv51XVJhtMo91sIcADfjSuJjw"
                    ),
                    CreateUserLogin(
                        prereleaseUser1Id,
                        "uLGzMPaxGz0nY6nbff7wkBP7ly2iLdephomGPFOP0k8"
                    ),                    
                    CreateUserLogin(
                        prereleaseUser2Id,
                        "s5vNxMDGwRCvg3MTtLEDomZqOKl7cvv2f8PW5NvJzbw"
                    )
                );
        }
    }
}

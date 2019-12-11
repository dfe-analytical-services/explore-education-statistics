using System;
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
            
            const string applicationUserRoleId = "bd1ed6af-6c0b-4550-90d6-bcce74d2e7a7";
            const string bauUserRoleId = "cf67b697-bddd-41bd-86e0-11b7e11d99b3";

            modelBuilder.Entity<IdentityRole>()
                .HasData(
                    CreateRole(
                        applicationUserRoleId,
                        "Application User"
                    )
                );

            modelBuilder.Entity<IdentityRoleClaim<string>>()
                .HasData(
                    new IdentityRoleClaim<string>
                    {
                        Id = -1,
                        RoleId = applicationUserRoleId,
                        ClaimType = SecurityClaimTypes.ApplicationAccessGranted.ToString(),
                        ClaimValue = "",
                    }
                );

            modelBuilder.Entity<IdentityRole>()
                .HasData(
                    CreateRole(
                        bauUserRoleId,
                        "BAU User"
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
                    }
                );
            
            const string analystUser1Id = "e7f7c82e-aaf3-43db-a5ab-755678f67d04";
            const string analystUser2Id = "6620bccf-2433-495e-995d-fc76c59d9c62";
            const string analystUser3Id = "b390b405-ef90-4b9d-8770-22948e53189a";

            const string bauUser1Id = "b99e8358-9a5e-4a3a-9288-6f94c7e1e3dd";
            const string bauUser2Id = "b6f0dfa5-0102-4b91-9aa8-f23b7d8aca63";
            
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
                    )
                );

            modelBuilder.Entity<IdentityUserRole<string>>()
                .HasData(
                    new IdentityUserRole<string>
                    {
                        UserId = analystUser1Id,
                        RoleId = applicationUserRoleId
                    },
                    new IdentityUserRole<string>
                    {
                        UserId = analystUser2Id,
                        RoleId = applicationUserRoleId
                    },
                    new IdentityUserRole<string>
                    {
                        UserId = analystUser3Id,
                        RoleId = applicationUserRoleId
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
                    )
                );
        }
    }
}

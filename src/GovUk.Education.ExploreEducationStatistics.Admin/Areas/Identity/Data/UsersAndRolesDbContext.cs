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

            const string primaryAnalyst1Id = "e7f7c82e-aaf3-43db-a5ab-755678f67d04";
            const string primaryAnalyst2Id = "6620bccf-2433-495e-995d-fc76c59d9c62";
            const string primaryAnalystRoleId = "bd1ed6af-6c0b-4550-90d6-bcce74d2e7a7";
            
            modelBuilder.Entity<ApplicationUser>()
                .HasData(
                    CreateApplicationUser(
                        primaryAnalyst1Id,
                        "primary.analyst1@example.com",
                        "Primary1",
                        "Analyst1"
                    ),
                    CreateApplicationUser(
                        primaryAnalyst2Id,
                        "primary.analyst2@example.com",
                        "Primary2",
                        "Analyst2"
                    )
                );

            modelBuilder.Entity<IdentityRole>()
                .HasData(
                    CreateRole(
                        primaryAnalystRoleId,
                        "Primary Analyst"
                    )
                );

            modelBuilder.Entity<IdentityRoleClaim<string>>()
                .HasData(
                    new IdentityRoleClaim<string>
                    {
                        Id = 1,
                        RoleId = primaryAnalystRoleId,
                        ClaimType = SecurityClaimTypes.AdminAccessGranted.ToString(),
                        ClaimValue = "",
                    }
                );

            modelBuilder.Entity<IdentityUserRole<string>>()
                .HasData(
                    new IdentityUserRole<string>
                    {
                        UserId = primaryAnalyst1Id,
                        RoleId = primaryAnalystRoleId
                    },
                    new IdentityUserRole<string>
                    {
                        UserId = primaryAnalyst2Id,
                        RoleId = primaryAnalystRoleId
                    }
                );

            modelBuilder.Entity<IdentityUserLogin<string>>()
                .HasData(
                    CreateUserLogin(
                        primaryAnalyst1Id,
                        "5zzTEeAYz71aVPJ1ho1VGW3cYk7_qcQpkDqYYxbH3po"
                    ),
                    CreateUserLogin(
                        primaryAnalyst2Id,
                        "RLdgJMsfN6QVjpCbkaOYIpzh6DA3QpRfnBcfIx46uDM"
                    )
                );
        }
    }
}

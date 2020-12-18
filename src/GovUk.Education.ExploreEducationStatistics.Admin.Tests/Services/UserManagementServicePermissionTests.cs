using System;
using GovUk.Education.ExploreEducationStatistics.Admin.Areas.Identity.Data;
using GovUk.Education.ExploreEducationStatistics.Admin.Areas.Identity.Data.Models;
using GovUk.Education.ExploreEducationStatistics.Admin.Models;
using GovUk.Education.ExploreEducationStatistics.Admin.Security;
using GovUk.Education.ExploreEducationStatistics.Admin.Services;
using GovUk.Education.ExploreEducationStatistics.Common.Services.Interfaces.Security;
using GovUk.Education.ExploreEducationStatistics.Common.Utils;
using GovUk.Education.ExploreEducationStatistics.Content.Model.Database;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using Moq;
using Xunit;
using static GovUk.Education.ExploreEducationStatistics.Admin.Tests.Services.PermissionTestUtil;

namespace GovUk.Education.ExploreEducationStatistics.Admin.Tests.Services
{
    public class UserManagementServicePermissionTest
    {
        [Fact]
        public void ListPendingInvitesAsync()
        {
            PolicyCheckBuilder()
                .ExpectCheck(SecurityPolicies.CanManageUsersOnSystem)
                .AssertSuccess(
                    async userService =>
                    {
                        await using (var userAndRolesDbContext =
                            DbUtils.InMemoryUserAndRolesDbContext(Guid.NewGuid().ToString()))
                        {
                            var userManagementService = BuildUserManagementService(
                                userAndRolesDbContext,
                                userService.Object);
                            return await userManagementService.ListPendingInvitesAsync();
                        }
                    });
        }

        [Fact]
        public void InviteUserAsync()
        {
            PolicyCheckBuilder()
                .ExpectCheck(SecurityPolicies.CanManageUsersOnSystem)
                .AssertSuccess(
                    async userService =>
                    {
                        var userEmail = "test@test.com";
                        var inviteCreatedByUser = "Test User";
                        var roleId = Guid.NewGuid().ToString();
                        var user = new ApplicationUser
                        {
                            FirstName = "TestFirstName",
                            LastName = "TestLastName",
                        };
                        var role = new IdentityRole
                        {
                            Id = roleId
                        };

                        var userAndRolesContextId = Guid.NewGuid().ToString();
                        await using (var userAndRolesDbContext =
                            DbUtils.InMemoryUserAndRolesDbContext(userAndRolesContextId))
                        {
                            await userAndRolesDbContext.AddAsync(user);
                            await userAndRolesDbContext.AddAsync(role);
                            await userAndRolesDbContext.SaveChangesAsync();
                        }

                        await using (var userAndRolesDbContext =
                            DbUtils.InMemoryUserAndRolesDbContext(userAndRolesContextId))
                        {
                            var userManagementService = BuildUserManagementService(
                                userAndRolesDbContext,
                                userService.Object);
                            return await userManagementService.InviteUserAsync(userEmail, inviteCreatedByUser, roleId);
                        }
                    });
        }

        [Fact]
        public void CancelUserInvite()
        {
            PolicyCheckBuilder()
                .ExpectCheck(SecurityPolicies.CanManageUsersOnSystem)
                .AssertSuccess(
                    async userService =>
                    {
                        var userEmail = "test@test.com";
                        var userInvite = new UserInvite
                        {
                            Email = userEmail,
                            Accepted = false,
                            Role = new IdentityRole(),
                            RoleId = Guid.Empty.ToString(),
                            Created = DateTime.UtcNow,
                            CreatedBy = "Test creator"
                        };

                        var userAndRolesContextId = Guid.NewGuid().ToString();
                        await using (var userAndRolesDbContext =
                            DbUtils.InMemoryUserAndRolesDbContext(userAndRolesContextId))
                        {
                            await userAndRolesDbContext.AddAsync(userInvite);
                            await userAndRolesDbContext.SaveChangesAsync();
                        }

                        await using (var userAndRolesDbContext =
                            DbUtils.InMemoryUserAndRolesDbContext(userAndRolesContextId))
                        {
                            var userManagementService = BuildUserManagementService(userAndRolesDbContext, userService.Object);
                            return await userManagementService.CancelInviteAsync(userEmail);
                        }
                    });
        }

        private static Mock<IConfiguration> ConfigurationMock()
        {
            var templateSection = new Mock<IConfigurationSection>();
            var adminUriSection = new Mock<IConfigurationSection>();
            var configuration = new Mock<IConfiguration>();

            templateSection.Setup(m => m.Value)
                .Returns("the-template-id");

            adminUriSection.Setup(m => m.Value)
                .Returns("admin-uri");

            configuration
                .Setup(m => m.GetSection("NotifyInviteTemplateId"))
                .Returns(templateSection.Object);

            configuration
                .Setup(m => m.GetSection("AdminUri"))
                .Returns(adminUriSection.Object);

            return configuration;
        }

        private static UserManagementService BuildUserManagementService(
            UsersAndRolesDbContext usersAndRolesDbContext,
            IUserService userService = null,
            ContentDbContext contentDbContext = null,
            IEmailService emailService = null,
            IConfiguration configuration = null,
            UserManager<ApplicationUser> userManager = null,
            IPersistenceHelper<ContentDbContext> persistenceHelper = null)
        {
            return new UserManagementService(
                usersAndRolesDbContext,
                userService ?? MockUtils.AlwaysTrueUserService().Object,
                contentDbContext ?? new Mock<ContentDbContext>().Object,
                emailService ?? new Mock<IEmailService>().Object,
                configuration ?? ConfigurationMock().Object,
                userManager,
                persistenceHelper ?? new Mock<IPersistenceHelper<ContentDbContext>>().Object);
        }
    }
}

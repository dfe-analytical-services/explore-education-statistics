using System;
using GovUk.Education.ExploreEducationStatistics.Admin.Areas.Identity.Data;
using GovUk.Education.ExploreEducationStatistics.Admin.Security;
using GovUk.Education.ExploreEducationStatistics.Admin.Services;
using GovUk.Education.ExploreEducationStatistics.Admin.Services.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Common.Services.Interfaces.Security;
using GovUk.Education.ExploreEducationStatistics.Common.Utils;
using GovUk.Education.ExploreEducationStatistics.Content.Model.Database;
using Moq;
using Xunit;
using static GovUk.Education.ExploreEducationStatistics.Admin.Security.SecurityPolicies;
using static GovUk.Education.ExploreEducationStatistics.Admin.Tests.Services.DbUtils;
using static GovUk.Education.ExploreEducationStatistics.Common.Tests.Utils.PermissionTestUtils;

namespace GovUk.Education.ExploreEducationStatistics.Admin.Tests.Services
{
    public class UserManagementServicePermissionTest
    {
        [Fact]
        public void ListAllUsers()
        {
            PolicyCheckBuilder<SecurityPolicies>()
                .ExpectCheckToFail(CanManageUsersOnSystem)
                .AssertForbidden(async userService =>
                {
                    var service = SetupUserManagementService(userService: userService.Object);
                    return await service.ListAllUsers();
                });
        }

        [Fact]
        public void ListPublications()
        {
            PolicyCheckBuilder<SecurityPolicies>()
                .ExpectCheckToFail(CanManageUsersOnSystem)
                .AssertForbidden(async userService =>
                {
                    var service = SetupUserManagementService(userService: userService.Object);
                    return await service.ListPublications();
                });
        }

        [Fact]
        public void ListReleases()
        {
            PolicyCheckBuilder<SecurityPolicies>()
                .ExpectCheckToFail(CanManageUsersOnSystem)
                .AssertForbidden(async userService =>
                {
                    var service = SetupUserManagementService(userService: userService.Object);
                    return await service.ListReleases();
                });
        }

        [Fact]
        public void ListRoles()
        {
            PolicyCheckBuilder<SecurityPolicies>()
                .ExpectCheckToFail(CanManageUsersOnSystem)
                .AssertForbidden(async userService =>
                {
                    var service = SetupUserManagementService(userService: userService.Object);
                    return await service.ListRoles();
                });
        }

        [Fact]
        public void GetUser()
        {
            PolicyCheckBuilder<SecurityPolicies>()
                .ExpectCheckToFail(CanManageUsersOnSystem)
                .AssertForbidden(async userService =>
                    {
                        var service = SetupUserManagementService(userService: userService.Object);
                        return await service.GetUser(Guid.NewGuid());
                    }
                );
        }

        [Fact]
        public void ListPendingInvites()
        {
            PolicyCheckBuilder<SecurityPolicies>()
                .ExpectCheckToFail(CanManageUsersOnSystem)
                .AssertForbidden(async userService =>
                {
                    var service = SetupUserManagementService(userService: userService.Object);
                    return await service.ListPendingInvites();
                });
        }

        [Fact]
        public void InviteUser()
        {
            PolicyCheckBuilder<SecurityPolicies>()
                .ExpectCheckToFail(CanManageUsersOnSystem)
                .AssertForbidden(async userService =>
                {
                    var service = SetupUserManagementService(userService: userService.Object);
                    return await service.InviteUser(
                        "test@test.com",
                        "Test User",
                        Guid.NewGuid().ToString());
                });
        }

        [Fact]
        public void CancelInvite()
        {
            PolicyCheckBuilder<SecurityPolicies>()
                .ExpectCheckToFail(CanManageUsersOnSystem)
                .AssertForbidden(async userService =>
                {
                    var service = SetupUserManagementService(userService: userService.Object);
                    return await service.CancelInvite("test@test.com");
                });
        }

        [Fact]
        public void UpdateUser()
        {
            PolicyCheckBuilder<SecurityPolicies>()
                .ExpectCheckToFail(CanManageUsersOnSystem)
                .AssertForbidden(async userService =>
                    {
                        var service = SetupUserManagementService(userService: userService.Object);
                        return await service.UpdateUser(Guid.NewGuid().ToString(), Guid.NewGuid().ToString());
                    }
                );
        }

        private static UserManagementService SetupUserManagementService(
            ContentDbContext contentDbContext = null,
            UsersAndRolesDbContext usersAndRolesDbContext = null,
            IPersistenceHelper<UsersAndRolesDbContext> usersAndRolesPersistenceHelper = null,
            IEmailTemplateService emailTemplateService = null,
            IUserService userService = null,
            IUserRoleService userRoleService = null)
        {
            contentDbContext ??= InMemoryApplicationDbContext();
            usersAndRolesDbContext ??= InMemoryUserAndRolesDbContext();

            return new UserManagementService(
                usersAndRolesDbContext ?? InMemoryUserAndRolesDbContext(),
                contentDbContext,
                usersAndRolesPersistenceHelper ?? new PersistenceHelper<UsersAndRolesDbContext>(usersAndRolesDbContext),
                emailTemplateService ?? new Mock<IEmailTemplateService>().Object,
                userRoleService ?? new Mock<IUserRoleService>().Object,
                userService ?? new Mock<IUserService>().Object
            );
        }
    }
}

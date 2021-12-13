#nullable enable
using System;
using System.Threading.Tasks;
using GovUk.Education.ExploreEducationStatistics.Admin.Areas.Identity.Data;
using GovUk.Education.ExploreEducationStatistics.Admin.Security;
using GovUk.Education.ExploreEducationStatistics.Admin.Services;
using GovUk.Education.ExploreEducationStatistics.Admin.Services.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Common.Services.Interfaces.Security;
using GovUk.Education.ExploreEducationStatistics.Common.Utils;
using GovUk.Education.ExploreEducationStatistics.Content.Model.Database;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Moq;
using Xunit;
using static GovUk.Education.ExploreEducationStatistics.Admin.Security.SecurityPolicies;
using static GovUk.Education.ExploreEducationStatistics.Admin.Tests.Services.DbUtils;
using static GovUk.Education.ExploreEducationStatistics.Common.Tests.Utils.MockUtils;
using static GovUk.Education.ExploreEducationStatistics.Common.Tests.Utils.PermissionTestUtils;
using static Moq.MockBehavior;

namespace GovUk.Education.ExploreEducationStatistics.Admin.Tests.Services
{
    public class UserManagementServicePermissionTest
    {
        [Fact]
        public async Task ListAllUsers()
        {
            await PolicyCheckBuilder<SecurityPolicies>()
                .ExpectCheckToFail(CanManageUsersOnSystem)
                .AssertForbidden(async userService =>
                {
                    var service = SetupUserManagementService(userService: userService.Object);
                    return await service.ListAllUsers();
                });
        }

        [Fact]
        public async Task ListPublications()
        {
            await PolicyCheckBuilder<SecurityPolicies>()
                .ExpectCheckToFail(CanManageUsersOnSystem)
                .AssertForbidden(async userService =>
                {
                    var service = SetupUserManagementService(userService: userService.Object);
                    return await service.ListPublications();
                });
        }

        [Fact]
        public async Task ListReleases()
        {
            await PolicyCheckBuilder<SecurityPolicies>()
                .ExpectCheckToFail(CanManageUsersOnSystem)
                .AssertForbidden(async userService =>
                {
                    var service = SetupUserManagementService(userService: userService.Object);
                    return await service.ListReleases();
                });
        }

        [Fact]
        public async Task ListRoles()
        {
            await PolicyCheckBuilder<SecurityPolicies>()
                .ExpectCheckToFail(CanManageUsersOnSystem)
                .AssertForbidden(async userService =>
                {
                    var service = SetupUserManagementService(userService: userService.Object);
                    return await service.ListRoles();
                });
        }

        [Fact]
        public async Task GetUser()
        {
            await PolicyCheckBuilder<SecurityPolicies>()
                .ExpectCheckToFail(CanManageUsersOnSystem)
                .AssertForbidden(async userService =>
                    {
                        var service = SetupUserManagementService(userService: userService.Object);
                        return await service.GetUser(Guid.NewGuid());
                    }
                );
        }

        [Fact]
        public async Task ListPendingInvites()
        {
            await PolicyCheckBuilder<SecurityPolicies>()
                .ExpectCheckToFail(CanManageUsersOnSystem)
                .AssertForbidden(async userService =>
                {
                    var service = SetupUserManagementService(userService: userService.Object);
                    return await service.ListPendingInvites();
                });
        }

        [Fact]
        public async Task InviteUser()
        {
            await PolicyCheckBuilder<SecurityPolicies>()
                .ExpectCheckToFail(CanManageUsersOnSystem)
                .AssertForbidden(async userService =>
                {
                    var service = SetupUserManagementService(userService: userService.Object);
                    return await service.InviteUser(
                        "test@test.com",
                        Guid.NewGuid().ToString());
                });
        }

        [Fact]
        public async Task CancelInvite()
        {
            await PolicyCheckBuilder<SecurityPolicies>()
                .ExpectCheckToFail(CanManageUsersOnSystem)
                .AssertForbidden(async userService =>
                {
                    var service = SetupUserManagementService(userService: userService.Object);
                    return await service.CancelInvite("test@test.com");
                });
        }

        [Fact]
        public async Task UpdateUser()
        {
            await PolicyCheckBuilder<SecurityPolicies>()
                .ExpectCheckToFail(CanManageUsersOnSystem)
                .AssertForbidden(async userService =>
                    {
                        var service = SetupUserManagementService(userService: userService.Object);
                        return await service.UpdateUser(Guid.NewGuid().ToString(), Guid.NewGuid().ToString());
                    }
                );
        }

        private static UserManagementService SetupUserManagementService(
            ContentDbContext? contentDbContext = null,
            UsersAndRolesDbContext? usersAndRolesDbContext = null,
            IPersistenceHelper<ContentDbContext>? contentPersistenceHelper = null,
            IPersistenceHelper<UsersAndRolesDbContext>? usersAndRolesPersistenceHelper = null,
            IEmailTemplateService? emailTemplateService = null,
            IUserRoleService? userRoleService = null,
            IUserRepository? userRepository = null,
            IUserService? userService = null,
            IUserInviteRepository? userInviteRepository = null,
            IUserReleaseInviteRepository? userReleaseInviteRepository = null,
            IUserReleaseRoleRepository? userReleaseRoleRepository = null,
            IConfiguration? configuration = null,
            IEmailService? emailService = null,
            IHttpContextAccessor? httpContextAccessor = null)
        {
            contentDbContext ??= InMemoryApplicationDbContext();
            usersAndRolesDbContext ??= InMemoryUserAndRolesDbContext();

            return new UserManagementService(
                usersAndRolesDbContext,
                contentDbContext,
                contentPersistenceHelper ?? new PersistenceHelper<ContentDbContext>(contentDbContext),
                usersAndRolesPersistenceHelper ?? new PersistenceHelper<UsersAndRolesDbContext>(usersAndRolesDbContext),
                emailTemplateService ?? new Mock<IEmailTemplateService>(Strict).Object,
                userRoleService ?? new Mock<IUserRoleService>(Strict).Object,
                userRepository ?? new UserRepository(contentDbContext),
                userService ?? AlwaysTrueUserService().Object,
                userInviteRepository ?? new UserInviteRepository(usersAndRolesDbContext),
                userReleaseInviteRepository ?? new UserReleaseInviteRepository(contentDbContext),
                userReleaseRoleRepository ?? new UserReleaseRoleRepository(contentDbContext),
                configuration ?? CreateMockConfiguration().Object,
                emailService ?? new Mock<IEmailService>(Strict).Object,
                httpContextAccessor ?? new Mock<IHttpContextAccessor>(Strict).Object
            );
        }
    }
}

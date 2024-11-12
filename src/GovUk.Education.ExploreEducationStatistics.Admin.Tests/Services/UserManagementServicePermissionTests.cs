#nullable enable
using System;
using System.Threading.Tasks;
using GovUk.Education.ExploreEducationStatistics.Admin.Database;
using GovUk.Education.ExploreEducationStatistics.Admin.Models;
using GovUk.Education.ExploreEducationStatistics.Admin.Security;
using GovUk.Education.ExploreEducationStatistics.Admin.Services;
using GovUk.Education.ExploreEducationStatistics.Admin.Services.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Admin.ViewModels;
using GovUk.Education.ExploreEducationStatistics.Common.Services.Interfaces.Security;
using GovUk.Education.ExploreEducationStatistics.Common.Utils;
using GovUk.Education.ExploreEducationStatistics.Content.Model;
using GovUk.Education.ExploreEducationStatistics.Content.Model.Database;
using Microsoft.AspNetCore.Identity;
using Moq;
using static GovUk.Education.ExploreEducationStatistics.Admin.Security.SecurityPolicies;
using static GovUk.Education.ExploreEducationStatistics.Admin.Tests.Services.DbUtils;
using static GovUk.Education.ExploreEducationStatistics.Admin.Tests.Utils.AdminMockUtils;
using static GovUk.Education.ExploreEducationStatistics.Common.Tests.Utils.MockUtils;
using static GovUk.Education.ExploreEducationStatistics.Common.Tests.Utils.PermissionTestUtils;
using static Moq.MockBehavior;
using IReleaseVersionRepository = GovUk.Education.ExploreEducationStatistics.Content.Model.Repository.Interfaces.IReleaseVersionRepository;
using ReleaseVersionRepository = GovUk.Education.ExploreEducationStatistics.Content.Model.Repository.ReleaseVersionRepository;

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
                    return await service.InviteUser(new UserInviteCreateRequest());
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

        public class DeleteUserTests
        {
            [Fact]
            public async Task Success()
            {
                await using var contentDbContext = DbUtils.InMemoryApplicationDbContext();
                contentDbContext.Users.Add(new User { Email = "ees-test.user@education.gov.uk"} );
                await contentDbContext.SaveChangesAsync();

                await PolicyCheckBuilder<SecurityPolicies>()
                    .SetupCheck(CanManageUsersOnSystem)
                    .AssertSuccess(async userService =>
                        {
                            userService.Setup(mock => mock.GetUserId())
                                .Returns(Guid.NewGuid());

                            var service = SetupUserManagementService(
                                contentDbContext: contentDbContext,
                                userService: userService.Object);
                            return await service.DeleteUser("ees-test.user@education.gov.uk");
                        }
                    );
            }
        }

        private static UserManagementService SetupUserManagementService(
            ContentDbContext? contentDbContext = null,
            UsersAndRolesDbContext? usersAndRolesDbContext = null,
            IPersistenceHelper<UsersAndRolesDbContext>? usersAndRolesPersistenceHelper = null,
            IEmailTemplateService? emailTemplateService = null,
            IReleaseVersionRepository? releaseVersionRepository = null,
            IUserRoleService? userRoleService = null,
            IUserService? userService = null,
            IUserInviteRepository? userInviteRepository = null,
            IUserReleaseInviteRepository? userReleaseInviteRepository = null,
            IUserPublicationInviteRepository? userPublicationInviteRepository = null,
            UserManager<ApplicationUser>? userManager = null,
            bool enableDeletion = false)
        {
            contentDbContext ??= InMemoryApplicationDbContext();
            usersAndRolesDbContext ??= InMemoryUserAndRolesDbContext();

            return new UserManagementService(
                usersAndRolesDbContext,
                contentDbContext,
                usersAndRolesPersistenceHelper ?? new PersistenceHelper<UsersAndRolesDbContext>(usersAndRolesDbContext),
                emailTemplateService ?? Mock.Of<IEmailTemplateService>(Strict),
                releaseVersionRepository ?? new ReleaseVersionRepository(contentDbContext),
                userRoleService ?? Mock.Of<IUserRoleService>(Strict),
                userService ?? AlwaysTrueUserService().Object,
                userInviteRepository ?? new UserInviteRepository(usersAndRolesDbContext),
                userReleaseInviteRepository ?? new UserReleaseInviteRepository(contentDbContext),
                userPublicationInviteRepository ?? new UserPublicationInviteRepository(contentDbContext),
                userManager ?? MockUserManager().Object
            );
        }
    }
}

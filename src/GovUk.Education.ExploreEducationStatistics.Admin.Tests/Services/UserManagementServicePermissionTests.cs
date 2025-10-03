#nullable enable
using GovUk.Education.ExploreEducationStatistics.Admin.Database;
using GovUk.Education.ExploreEducationStatistics.Admin.Models;
using GovUk.Education.ExploreEducationStatistics.Admin.Security;
using GovUk.Education.ExploreEducationStatistics.Admin.Services;
using GovUk.Education.ExploreEducationStatistics.Admin.Services.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Admin.ViewModels;
using GovUk.Education.ExploreEducationStatistics.Common.Services.Interfaces.Security;
using GovUk.Education.ExploreEducationStatistics.Common.Tests.Fixtures;
using GovUk.Education.ExploreEducationStatistics.Common.Utils;
using GovUk.Education.ExploreEducationStatistics.Content.Model.Database;
using GovUk.Education.ExploreEducationStatistics.Content.Model.Tests.Fixtures;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
using Moq;
using static GovUk.Education.ExploreEducationStatistics.Admin.Security.SecurityPolicies;
using static GovUk.Education.ExploreEducationStatistics.Admin.Tests.Services.DbUtils;
using static GovUk.Education.ExploreEducationStatistics.Admin.Tests.Utils.AdminMockUtils;
using static GovUk.Education.ExploreEducationStatistics.Common.Tests.Utils.MockUtils;
using static GovUk.Education.ExploreEducationStatistics.Common.Tests.Utils.PermissionTestUtils;
using static Moq.MockBehavior;

namespace GovUk.Education.ExploreEducationStatistics.Admin.Tests.Services;

public class UserManagementServicePermissionTests
{
    private readonly DataFixture _dataFixture = new();

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
            });
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
            });
    }

    [Fact]
    public async Task Delete_User_Success()
    {
        await using var contentDbContext = DbUtils.InMemoryApplicationDbContext();
        contentDbContext.Users.Add(_dataFixture.DefaultUser().WithEmail("ees-test.user@education.gov.uk"));
        await contentDbContext.SaveChangesAsync();

        await PolicyCheckBuilder<SecurityPolicies>()
            .SetupCheck(CanManageUsersOnSystem)
            .AssertSuccess(async userService =>
            {
                userService.Setup(mock => mock.GetUserId()).Returns(Guid.NewGuid());

                var service = SetupUserManagementService(
                    contentDbContext: contentDbContext,
                    userService: userService.Object
                );
                return await service.DeleteUser("ees-test.user@education.gov.uk");
            });
    }

    private static UserManagementService SetupUserManagementService(
        ContentDbContext? contentDbContext = null,
        UsersAndRolesDbContext? usersAndRolesDbContext = null,
        IPersistenceHelper<UsersAndRolesDbContext>? usersAndRolesPersistenceHelper = null,
        IEmailTemplateService? emailTemplateService = null,
        IUserRoleService? userRoleService = null,
        IUserRepository? userRepository = null,
        IUserService? userService = null,
        IUserInviteRepository? userInviteRepository = null,
        IUserReleaseInviteRepository? userReleaseInviteRepository = null,
        IUserPublicationInviteRepository? userPublicationInviteRepository = null,
        IUserReleaseRoleRepository? userReleaseRoleRepository = null,
        IUserPublicationRoleRepository? userPublicationRoleRepository = null,
        UserManager<ApplicationUser>? userManager = null
    )
    {
        contentDbContext ??= InMemoryApplicationDbContext();
        usersAndRolesDbContext ??= InMemoryUserAndRolesDbContext();
        userRepository ??= new UserRepository(contentDbContext);

        userReleaseRoleRepository ??= new UserReleaseRoleRepository(
            contentDbContext,
            logger: Mock.Of<ILogger<UserReleaseRoleRepository>>()
        );

        userPublicationRoleRepository ??= new UserPublicationRoleRepository(contentDbContext);

        return new UserManagementService(
            usersAndRolesDbContext,
            contentDbContext,
            usersAndRolesPersistenceHelper ?? new PersistenceHelper<UsersAndRolesDbContext>(usersAndRolesDbContext),
            emailTemplateService ?? Mock.Of<IEmailTemplateService>(Strict),
            userRoleService ?? Mock.Of<IUserRoleService>(Strict),
            userRepository,
            userService ?? AlwaysTrueUserService().Object,
            userInviteRepository ?? new UserInviteRepository(usersAndRolesDbContext),
            userReleaseInviteRepository
                ?? new UserReleaseInviteRepository(
                    contentDbContext: contentDbContext,
                    logger: Mock.Of<ILogger<UserReleaseInviteRepository>>()
                ),
            userPublicationInviteRepository ?? new UserPublicationInviteRepository(contentDbContext),
            userReleaseRoleRepository,
            userPublicationRoleRepository,
            userManager ?? MockUserManager().Object
        );
    }
}

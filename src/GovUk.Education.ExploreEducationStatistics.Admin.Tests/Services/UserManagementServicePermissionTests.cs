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
using GovUk.Education.ExploreEducationStatistics.Content.Model;
using GovUk.Education.ExploreEducationStatistics.Content.Model.Database;
using GovUk.Education.ExploreEducationStatistics.Content.Model.Tests.Fixtures;
using Microsoft.AspNetCore.Identity;
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
    private static readonly Guid CreatedById = Guid.NewGuid();

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
    public async Task DeleteUser_Success()
    {
        var user = _dataFixture.DefaultUser().Generate();

        var identityUser = new ApplicationUser { Email = user.Email };

        var usersAndRolesDbContextId = Guid.NewGuid().ToString();

        await using (var usersAndRolesDbContext = InMemoryUserAndRolesDbContext(usersAndRolesDbContextId))
        {
            usersAndRolesDbContext.Users.Add(identityUser);
            await usersAndRolesDbContext.SaveChangesAsync();
        }

        await PolicyCheckBuilder<SecurityPolicies>()
            .SetupCheck(CanManageUsersOnSystem)
            .AssertSuccess(async userService =>
            {
                await using var contentDbContext = InMemoryApplicationDbContext();
                await using var usersAndRolesDbContext = InMemoryUserAndRolesDbContext(usersAndRolesDbContextId);

                userService.Setup(mock => mock.GetUserId()).Returns(CreatedById);

                var userPublicationInviteRepository = new Mock<IUserPublicationInviteRepository>(Strict);
                userPublicationInviteRepository
                    .Setup(mock => mock.RemoveByUserEmail(user.Email, It.IsAny<CancellationToken>()))
                    .Returns(Task.CompletedTask);

                var userReleaseInviteRepository = new Mock<IUserReleaseInviteRepository>(Strict);
                userReleaseInviteRepository
                    .Setup(mock => mock.RemoveByUserEmail(user.Email, It.IsAny<CancellationToken>()))
                    .Returns(Task.CompletedTask);

                var userReleaseRoleRepository = new Mock<IUserReleaseRoleRepository>(Strict);
                userReleaseRoleRepository
                    .Setup(mock => mock.RemoveForUser(user.Id, It.IsAny<CancellationToken>()))
                    .Returns(Task.CompletedTask);

                var userPublicationRoleRepository = new Mock<IUserPublicationRoleRepository>(Strict);
                userPublicationRoleRepository
                    .Setup(mock => mock.RemoveForUser(user.Id, It.IsAny<CancellationToken>()))
                    .Returns(Task.CompletedTask);

                var userRepository = new Mock<IUserRepository>(Strict);
                userRepository
                    .Setup(mock => mock.FindActiveUserByEmail(user.Email, It.IsAny<CancellationToken>()))
                    .ReturnsAsync(user);
                userRepository
                    .Setup(mock => mock.SoftDeleteUser(user.Id, CreatedById, It.IsAny<CancellationToken>()))
                    .Returns(Task.CompletedTask);

                var userManager = MockUserManager();
                userManager
                    .Setup(mock => mock.DeleteAsync(It.Is<ApplicationUser>(u => u.Id == identityUser.Id)))
                    .ReturnsAsync(new IdentityResult());

                var service = SetupUserManagementService(
                    contentDbContext: contentDbContext,
                    usersAndRolesDbContext: usersAndRolesDbContext,
                    userService: userService.Object,
                    userRepository: userRepository.Object,
                    userPublicationInviteRepository: userPublicationInviteRepository.Object,
                    userReleaseInviteRepository: userReleaseInviteRepository.Object,
                    userPublicationRoleRepository: userPublicationRoleRepository.Object,
                    userReleaseRoleRepository: userReleaseRoleRepository.Object,
                    userManager: userManager.Object
                );

                var result = await service.DeleteUser(user.Email);

                VerifyAllMocks(
                    userRepository,
                    userPublicationInviteRepository,
                    userReleaseInviteRepository,
                    userPublicationRoleRepository,
                    userReleaseRoleRepository,
                    userManager
                );

                return result;
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
        IUserReleaseInviteRepository? userReleaseInviteRepository = null,
        IUserPublicationInviteRepository? userPublicationInviteRepository = null,
        IUserReleaseRoleRepository? userReleaseRoleRepository = null,
        IUserPublicationRoleRepository? userPublicationRoleRepository = null,
        UserManager<ApplicationUser>? userManager = null
    )
    {
        contentDbContext ??= InMemoryApplicationDbContext();
        usersAndRolesDbContext ??= InMemoryUserAndRolesDbContext();

        return new UserManagementService(
            usersAndRolesDbContext,
            contentDbContext,
            usersAndRolesPersistenceHelper ?? new PersistenceHelper<UsersAndRolesDbContext>(usersAndRolesDbContext),
            emailTemplateService ?? Mock.Of<IEmailTemplateService>(Strict),
            userRoleService ?? Mock.Of<IUserRoleService>(Strict),
            userRepository ?? Mock.Of<IUserRepository>(Strict),
            userService ?? AlwaysTrueUserService(CreatedById).Object,
            userReleaseInviteRepository ?? Mock.Of<IUserReleaseInviteRepository>(Strict),
            userPublicationInviteRepository ?? Mock.Of<IUserPublicationInviteRepository>(Strict),
            userReleaseRoleRepository ?? Mock.Of<IUserReleaseRoleRepository>(Strict),
            userPublicationRoleRepository ?? Mock.Of<IUserPublicationRoleRepository>(Strict),
            userManager ?? MockUserManager().Object
        );
    }
}

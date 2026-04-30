#nullable enable
using GovUk.Education.ExploreEducationStatistics.Admin.Database;
using GovUk.Education.ExploreEducationStatistics.Admin.Models;
using GovUk.Education.ExploreEducationStatistics.Admin.Security;
using GovUk.Education.ExploreEducationStatistics.Admin.Services;
using GovUk.Education.ExploreEducationStatistics.Admin.Services.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Common.Services.Interfaces.Security;
using GovUk.Education.ExploreEducationStatistics.Common.Utils;
using GovUk.Education.ExploreEducationStatistics.Content.Model;
using GovUk.Education.ExploreEducationStatistics.Content.Model.Database;
using Microsoft.AspNetCore.Identity;
using Moq;
using static GovUk.Education.ExploreEducationStatistics.Admin.Security.SecurityPolicies;
using static GovUk.Education.ExploreEducationStatistics.Admin.Tests.Services.DbUtils;
using static GovUk.Education.ExploreEducationStatistics.Admin.Tests.Utils.AdminMockUtils;
using static GovUk.Education.ExploreEducationStatistics.Common.Tests.Utils.PermissionTestUtils;

namespace GovUk.Education.ExploreEducationStatistics.Admin.Tests.Services;

public class UserRoleServicePermissionTests
{
    private readonly Publication _publication = new() { Id = Guid.NewGuid() };

    [Fact]
    public async Task SetGlobalRoleForUser()
    {
        await PolicyCheckBuilder<SecurityPolicies>()
            .ExpectCheckToFail(CanManageUsersOnSystem)
            .AssertForbidden(async userService =>
            {
                var service = SetupService(userService: userService.Object);
                return await service.SetGlobalRoleForUser(Guid.NewGuid().ToString(), Guid.NewGuid().ToString());
            });
    }

    [Fact]
    public async Task AddPublicationRole()
    {
        await PolicyCheckBuilder<SecurityPolicies>()
            .ExpectCheckToFail(CanManageUsersOnSystem)
            .AssertForbidden(async userService =>
            {
                var service = SetupService(userService: userService.Object);
                return await service.AddPublicationRole(Guid.NewGuid(), Guid.NewGuid(), PublicationRole.Drafter);
            });
    }

    [Fact]
    public async Task GetAllGlobalRoles()
    {
        await PolicyCheckBuilder<SecurityPolicies>()
            .ExpectCheckToFail(CanManageUsersOnSystem)
            .AssertForbidden(async userService =>
            {
                var service = SetupService(userService: userService.Object);
                return await service.GetAllGlobalRoles();
            });
    }

    [Fact]
    public async Task GetGlobalRolesForUser()
    {
        await PolicyCheckBuilder<SecurityPolicies>()
            .ExpectCheckToFail(CanManageUsersOnSystem)
            .AssertForbidden(async userService =>
            {
                var service = SetupService(userService: userService.Object);
                return await service.GetGlobalRolesForUser(Guid.NewGuid().ToString());
            });
    }

    [Fact]
    public async Task GetPublicationRolesForUser()
    {
        await PolicyCheckBuilder<SecurityPolicies>()
            .ExpectCheckToFail(CanManageUsersOnSystem)
            .AssertForbidden(async userService =>
            {
                var service = SetupService(userService: userService.Object);
                return await service.GetPublicationRolesForUser(Guid.NewGuid());
            });
    }

    [Fact]
    public async Task GetPublicationRolesForPublication()
    {
        await PolicyCheckBuilder<SecurityPolicies>()
            .SetupResourceCheckToFail(_publication, CanViewSpecificPublication)
            .AssertForbidden(async userService =>
            {
                await using var contentDbContext = InMemoryApplicationDbContext();
                await contentDbContext.AddAsync(_publication);
                await contentDbContext.SaveChangesAsync();

                var service = SetupService(contentDbContext: contentDbContext, userService: userService.Object);

                return await service.GetPublicationRolesForPublication(_publication.Id);
            });
    }

    [Fact]
    public async Task InviteDrafter()
    {
        await PolicyCheckBuilder<SecurityPolicies>()
            .SetupResourceCheckToFail(_publication, CanUpdateDrafters)
            .AssertForbidden(async userService =>
            {
                await using var contentDbContext = InMemoryApplicationDbContext();
                await contentDbContext.AddAsync(_publication);
                await contentDbContext.SaveChangesAsync();

                var service = SetupService(contentDbContext: contentDbContext, userService: userService.Object);
                return await service.InviteDrafter("test@test.com", _publication.Id);
            });
    }

    [Fact]
    public async Task UpdatePublicationDrafters()
    {
        await PolicyCheckBuilder<SecurityPolicies>()
            .SetupResourceCheckToFail(_publication, CanUpdateDrafters)
            .AssertForbidden(async userService =>
            {
                await using var contentDbContext = InMemoryApplicationDbContext();
                await contentDbContext.AddAsync(_publication);
                await contentDbContext.SaveChangesAsync();

                var service = SetupService(contentDbContext: contentDbContext, userService: userService.Object);
                return await service.UpdatePublicationDrafters(_publication.Id, []);
            });
    }

    [Fact]
    public async Task RemoveUserPublicationRole()
    {
        await PolicyCheckBuilder<SecurityPolicies>()
            .ExpectCheckToFail(CanManageUsersOnSystem)
            .AssertForbidden(async userService =>
            {
                var service = SetupService(userService: userService.Object);
                return await service.RemoveUserPublicationRole(Guid.NewGuid());
            });
    }

    [Fact]
    public async Task RemoveAllUserResourceRoles()
    {
        await PolicyCheckBuilder<SecurityPolicies>()
            .ExpectCheckToFail(CanManageUsersOnSystem)
            .AssertForbidden(async userService =>
            {
                var service = SetupService(userService: userService.Object);
                return await service.RemoveAllUserResourceRoles(Guid.NewGuid());
            });
    }

    private static UserRoleService SetupService(
        ContentDbContext? contentDbContext = null,
        UsersAndRolesDbContext? usersAndRolesDbContext = null,
        IPersistenceHelper<ContentDbContext>? contentPersistenceHelper = null,
        IPersistenceHelper<UsersAndRolesDbContext>? usersAndRolesPersistenceHelper = null,
        IUserResourceRoleNotificationService? userResourceRoleNotificationService = null,
        IUserPublicationRoleRepository? userPublicationRoleRepository = null,
        IUserPreReleaseRoleRepository? userPreReleaseRoleRepository = null,
        IUserRepository? userRepository = null,
        UserManager<ApplicationUser>? userManager = null,
        IUserService? userService = null
    )
    {
        contentDbContext ??= InMemoryApplicationDbContext();
        usersAndRolesDbContext ??= InMemoryUserAndRolesDbContext();

        return new UserRoleService(
            usersAndRolesDbContext,
            contentDbContext,
            contentPersistenceHelper ?? new PersistenceHelper<ContentDbContext>(contentDbContext),
            usersAndRolesPersistenceHelper ?? new PersistenceHelper<UsersAndRolesDbContext>(usersAndRolesDbContext),
            userResourceRoleNotificationService ?? Mock.Of<IUserResourceRoleNotificationService>(MockBehavior.Strict),
            userService ?? Mock.Of<IUserService>(MockBehavior.Strict),
            userPublicationRoleRepository ?? Mock.Of<IUserPublicationRoleRepository>(MockBehavior.Strict),
            userPreReleaseRoleRepository ?? Mock.Of<IUserPreReleaseRoleRepository>(MockBehavior.Strict),
            userRepository ?? Mock.Of<IUserRepository>(MockBehavior.Strict),
            userManager ?? MockUserManager().Object
        );
    }
}

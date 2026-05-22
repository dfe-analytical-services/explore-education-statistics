#nullable enable
using GovUk.Education.ExploreEducationStatistics.Admin.Database;
using GovUk.Education.ExploreEducationStatistics.Admin.Models;
using GovUk.Education.ExploreEducationStatistics.Admin.Security;
using GovUk.Education.ExploreEducationStatistics.Admin.Services;
using GovUk.Education.ExploreEducationStatistics.Admin.Services.Enums;
using GovUk.Education.ExploreEducationStatistics.Admin.Services.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Admin.Tests.Services.Extensions;
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
using static GovUk.Education.ExploreEducationStatistics.Common.Tests.Utils.PermissionTestUtils;

namespace GovUk.Education.ExploreEducationStatistics.Admin.Tests.Services;

public class UserRoleServicePermissionTests
{
    private readonly DataFixture _dataFixture = new();

    private readonly Publication _publication = new DataFixture().DefaultPublication();

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
    public async Task GetPublicationRoleInvitesForPublication()
    {
        await PolicyCheckBuilder<SecurityPolicies>()
            .SetupResourceCheckToFail(_publication, CanViewSpecificPublication)
            .AssertForbidden(async userService =>
            {
                await using var contentDbContext = InMemoryApplicationDbContext();
                await contentDbContext.AddAsync(_publication);
                await contentDbContext.SaveChangesAsync();

                var service = SetupService(contentDbContext: contentDbContext, userService: userService.Object);

                return await service.GetPublicationRoleInvitesForPublication(_publication.Id);
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
    public async Task RemoveDrafter()
    {
        UserPublicationRole userDrafterRole = _dataFixture
            .DefaultUserPublicationRole()
            .WithPublication(_publication)
            .WithRole(PublicationRole.Drafter);

        await PolicyCheckBuilder<SecurityPolicies>()
            .SetupResourceCheckToFail(_publication, CanUpdateDrafters)
            .AssertForbidden(async userService =>
            {
                var userPublicationRoleRepository = new Mock<IUserPublicationRoleRepository>(MockBehavior.Strict);
                userPublicationRoleRepository.SetupQuery(ResourceRoleFilter.All, userDrafterRole);

                var service = SetupService(
                    userService: userService.Object,
                    userPublicationRoleRepository: userPublicationRoleRepository.Object
                );
                return await service.RemoveDrafter(userDrafterRole.Id);
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
        IUserService? userService = null,
        IGlobalRoleService? globalRoleService = null
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
            userManager ?? MockUserManager().Object,
            globalRoleService ?? Mock.Of<IGlobalRoleService>(MockBehavior.Strict)
        );
    }
}

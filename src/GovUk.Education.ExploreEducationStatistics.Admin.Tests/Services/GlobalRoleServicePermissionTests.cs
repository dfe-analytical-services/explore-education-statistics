#nullable enable
using GovUk.Education.ExploreEducationStatistics.Admin.Database;
using GovUk.Education.ExploreEducationStatistics.Admin.Models;
using GovUk.Education.ExploreEducationStatistics.Admin.Security;
using GovUk.Education.ExploreEducationStatistics.Admin.Services;
using GovUk.Education.ExploreEducationStatistics.Admin.Services.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Common.Services.Interfaces.Security;
using GovUk.Education.ExploreEducationStatistics.Common.Utils;
using Microsoft.AspNetCore.Identity;
using Moq;
using static GovUk.Education.ExploreEducationStatistics.Admin.Security.SecurityPolicies;
using static GovUk.Education.ExploreEducationStatistics.Admin.Tests.Services.DbUtils;
using static GovUk.Education.ExploreEducationStatistics.Admin.Tests.Utils.AdminMockUtils;
using static GovUk.Education.ExploreEducationStatistics.Common.Tests.Utils.PermissionTestUtils;

namespace GovUk.Education.ExploreEducationStatistics.Admin.Tests.Services;

public class GlobalRoleServicePermissionTests
{
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

    private static GlobalRoleService SetupService(
        UsersAndRolesDbContext? usersAndRolesDbContext = null,
        IPersistenceHelper<UsersAndRolesDbContext>? usersAndRolesPersistenceHelper = null,
        IUserPublicationRoleRepository? userPublicationRoleRepository = null,
        IUserPreReleaseRoleRepository? userPreReleaseRoleRepository = null,
        UserManager<ApplicationUser>? userManager = null,
        IUserService? userService = null
    )
    {
        usersAndRolesDbContext ??= InMemoryUserAndRolesDbContext();

        return new GlobalRoleService(
            usersAndRolesDbContext,
            usersAndRolesPersistenceHelper ?? new PersistenceHelper<UsersAndRolesDbContext>(usersAndRolesDbContext),
            userService ?? Mock.Of<IUserService>(MockBehavior.Strict),
            userPublicationRoleRepository ?? Mock.Of<IUserPublicationRoleRepository>(MockBehavior.Strict),
            userPreReleaseRoleRepository ?? Mock.Of<IUserPreReleaseRoleRepository>(MockBehavior.Strict),
            userManager ?? MockUserManager().Object
        );
    }
}

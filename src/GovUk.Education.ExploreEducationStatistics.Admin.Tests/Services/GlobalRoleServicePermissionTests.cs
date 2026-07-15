#nullable enable
using GovUk.Education.ExploreEducationStatistics.Admin.Database;
using GovUk.Education.ExploreEducationStatistics.Admin.Models;
using GovUk.Education.ExploreEducationStatistics.Admin.Security;
using GovUk.Education.ExploreEducationStatistics.Admin.Services;
using GovUk.Education.ExploreEducationStatistics.Common.Services.Interfaces.Security;
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
                return await service.UpdateGlobalRoleForUser(Guid.NewGuid(), GlobalRoles.Role.BauUser);
            });
    }

    private static GlobalRoleService SetupService(
        UsersAndRolesDbContext? usersAndRolesDbContext = null,
        UserManager<ApplicationUser>? identityUserManager = null,
        IUserService? userService = null
    )
    {
        usersAndRolesDbContext ??= InMemoryUserAndRolesDbContext();

        return new GlobalRoleService(
            usersAndRolesDbContext,
            userService ?? Mock.Of<IUserService>(MockBehavior.Strict),
            identityUserManager ?? MockUserManager().Object
        );
    }
}

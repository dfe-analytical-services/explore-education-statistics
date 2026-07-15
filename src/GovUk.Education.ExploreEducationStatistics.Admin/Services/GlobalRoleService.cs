#nullable enable
using GovUk.Education.ExploreEducationStatistics.Admin.Database;
using GovUk.Education.ExploreEducationStatistics.Admin.Models;
using GovUk.Education.ExploreEducationStatistics.Admin.Services.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Admin.Services.Interfaces.Security;
using GovUk.Education.ExploreEducationStatistics.Common.Extensions;
using GovUk.Education.ExploreEducationStatistics.Common.Model;
using GovUk.Education.ExploreEducationStatistics.Common.Services.Interfaces.Security;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using static GovUk.Education.ExploreEducationStatistics.Admin.Models.GlobalRoles;

namespace GovUk.Education.ExploreEducationStatistics.Admin.Services;

public class GlobalRoleService(
    UsersAndRolesDbContext usersAndRolesDbContext,
    IUserService userService,
    UserManager<ApplicationUser> identityUserManager
) : IGlobalRoleService
{
    public async Task<Either<ActionResult, Unit>> UpdateGlobalRoleForUser(Guid userId, Role newRole) =>
        await userService
            .CheckCanManageAllUsers()
            .OnSuccess(async () =>
                await usersAndRolesDbContext.Users.SingleOrNotFoundAsync(u => u.Id == userId.ToString())
            )
            .OnSuccessVoid(async user => await SetExclusiveGlobalRoleIfRequired(user, newRole));

    private async Task SetExclusiveGlobalRoleIfRequired(ApplicationUser user, Role role)
    {
        var roleName = role.GetEnumLabel();
        var currentGlobalRoleNameForUser = await GetGlobalRoleNameForUser(user);

        if (currentGlobalRoleNameForUser is null)
        {
            await identityUserManager.AddToRoleAsync(user, roleName);
            return;
        }

        await identityUserManager.AddToRoleAsync(user, roleName);
        await identityUserManager.RemoveFromRoleAsync(user, currentGlobalRoleNameForUser);
    }

    private async Task<string?> GetGlobalRoleNameForUser(ApplicationUser user)
    {
        var existingRoleNames = await identityUserManager.GetRolesAsync(user) ?? [];

        // Should only ever be a maximum of one global role for a user.
        return existingRoleNames.SingleOrDefault();
    }
}

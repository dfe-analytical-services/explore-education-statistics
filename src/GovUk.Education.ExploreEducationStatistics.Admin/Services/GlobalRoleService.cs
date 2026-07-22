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
        var currentRole = await GetGlobalRoleNameForUser(user);

        if (currentRole == roleName)
        {
            return;
        }

        var addResult = await identityUserManager.AddToRoleAsync(user, roleName);

        if (!addResult.Succeeded)
        {
            throw new InvalidOperationException(
                $"Failed to add user '{user.Id}' to role '{roleName}'. "
                    + $"Errors: {string.Join(", ", addResult.Errors.Select(e => e.Description))}"
            );
        }

        if (currentRole is null)
        {
            return;
        }

        var removeResult = await identityUserManager.RemoveFromRoleAsync(user, currentRole);

        if (!removeResult.Succeeded)
        {
            throw new InvalidOperationException(
                $"Failed to remove user '{user.Id}' from role '{currentRole}'. "
                    + $"Errors: {string.Join(", ", removeResult.Errors.Select(e => e.Description))}"
            );
        }
    }

    private async Task<string?> GetGlobalRoleNameForUser(ApplicationUser user)
    {
        var existingRoleNames = await identityUserManager.GetRolesAsync(user) ?? [];

        // Should only ever be a maximum of one global role for a user.
        return existingRoleNames.SingleOrDefault();
    }
}

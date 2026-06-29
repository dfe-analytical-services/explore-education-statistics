#nullable enable
using GovUk.Education.ExploreEducationStatistics.Admin.Database;
using GovUk.Education.ExploreEducationStatistics.Admin.Models;
using GovUk.Education.ExploreEducationStatistics.Admin.Services.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Admin.Services.Interfaces.Security;
using GovUk.Education.ExploreEducationStatistics.Admin.ViewModels;
using GovUk.Education.ExploreEducationStatistics.Common.Extensions;
using GovUk.Education.ExploreEducationStatistics.Common.Model;
using GovUk.Education.ExploreEducationStatistics.Common.Services.Interfaces.Security;
using GovUk.Education.ExploreEducationStatistics.Common.Utils;
using GovUk.Education.ExploreEducationStatistics.Content.Model.Queries;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using static GovUk.Education.ExploreEducationStatistics.Admin.Models.GlobalRoles;

namespace GovUk.Education.ExploreEducationStatistics.Admin.Services;

public class GlobalRoleService(
    UsersAndRolesDbContext usersAndRolesDbContext,
    IPersistenceHelper<UsersAndRolesDbContext> usersAndRolesPersistenceHelper,
    IUserService userService,
    IUserPublicationRoleRepository userPublicationRoleRepository,
    IUserPreReleaseRoleRepository userPreReleaseRoleRepository,
    UserManager<ApplicationUser> identityUserManager
) : IGlobalRoleService
{
    public async Task<Either<ActionResult, List<RoleViewModel>>> GetAllGlobalRoles() =>
        await userService
            .CheckCanManageAllUsers()
            .OnSuccess(async () =>
            {
                return await usersAndRolesDbContext
                    .Roles.AsQueryable()
                    .Select(r => new RoleViewModel
                    {
                        Id = r.Id,
                        Name = r.Name,
                        NormalizedName = r.NormalizedName,
                    })
                    .OrderBy(x => x.Name)
                    .ToListAsync();
            });

    public async Task<Either<ActionResult, List<RoleViewModel>>> GetGlobalRolesForUser(string userId) =>
        await userService
            .CheckCanManageAllUsers()
            .OnSuccess(_ => usersAndRolesPersistenceHelper.CheckEntityExists<ApplicationUser, string>(userId))
            .OnSuccess(async () =>
            {
                var roleIds = await usersAndRolesDbContext
                    .UserRoles.AsQueryable()
                    .Where(r => r.UserId == userId)
                    .Select(r => r.RoleId)
                    .ToListAsync();

                return await usersAndRolesDbContext
                    .Roles.AsQueryable()
                    .Where(r => roleIds.Contains(r.Id))
                    .OrderBy(r => r.Name)
                    .Select(r => new RoleViewModel
                    {
                        Id = r.Id,
                        Name = r.Name,
                        NormalizedName = r.NormalizedName,
                    })
                    .ToListAsync();
            });

    public async Task<Either<ActionResult, Unit>> SetGlobalRoleForUser(string userId, string roleId) =>
        await userService
            .CheckCanManageAllUsers()
            .OnSuccess(async () =>
            {
                return await usersAndRolesPersistenceHelper
                    .CheckEntityExists<ApplicationUser, string>(userId)
                    .OnSuccessCombineWith(_ =>
                        usersAndRolesPersistenceHelper.CheckEntityExists<IdentityRole, string>(roleId)
                    )
                    .OnSuccessVoid(async tuple =>
                    {
                        var (user, role) = tuple;
                        await SetExclusiveGlobalRole(role.Name, user);
                    });
            });

    public async Task UpgradeToGlobalRoleIfRequired(ApplicationUser user, string globalRoleNameToSet)
    {
        var existingRoleNames = await identityUserManager.GetRolesAsync(user) ?? [];

        var userAlreadyAssignedToRole = existingRoleNames.Contains(globalRoleNameToSet);

        var higherRoleAlreadyAssigned = !existingRoleNames
            .Intersect(GetHigherRoles(globalRoleNameToSet))
            .IsNullOrEmpty();

        if (!userAlreadyAssignedToRole && !higherRoleAlreadyAssigned)
        {
            await identityUserManager.AddToRoleAsync(user, globalRoleNameToSet);
        }

        var lowerRolesToRemove = GetLowerRoles(globalRoleNameToSet).Intersect(existingRoleNames).ToList();

        if (lowerRolesToRemove.Count > 0)
        {
            await identityUserManager.RemoveFromRolesAsync(user, lowerRolesToRemove);
        }
    }

    public async Task DowngradeFromGlobalRoleIfRequired(ApplicationUser user, string globalRoleNameToDowngradeFrom)
    {
        var existingGlobalRoleNames = await identityUserManager.GetRolesAsync(user) ?? [];

        var higherPrecedenceExistingGlobalRoleNames = existingGlobalRoleNames.Where(role =>
            GlobalRolePrecedenceOrder.IndexOf(role) > GlobalRolePrecedenceOrder.IndexOf(globalRoleNameToDowngradeFrom)
        );

        var requiredGlobalRoleNames = await GetRequiredGlobalRoleNamesForResourceRoles(user);

        var highestPrecedenceRoleNameToRetain = higherPrecedenceExistingGlobalRoleNames
            .Concat(requiredGlobalRoleNames)
            .OrderBy(GlobalRolePrecedenceOrder.IndexOf)
            .LastOrDefault();

        await SetExclusiveGlobalRole(highestPrecedenceRoleNameToRetain, user);
    }

    private async Task<List<string>> GetRequiredGlobalRoleNamesForResourceRoles(ApplicationUser user)
    {
        var userId = Guid.Parse(user.Id);

        var userHasAPreReleaseRole = await userPreReleaseRoleRepository.Query().WhereForUser(userId).AnyAsync();

        var userHasAPublicationRole = await userPublicationRoleRepository.Query().WhereForUser(userId).AnyAsync();

        var globalRoles = new List<string>();

        if (userHasAPreReleaseRole)
        {
            globalRoles.Add(RoleNames.PrereleaseUser);
        }

        if (userHasAPublicationRole)
        {
            globalRoles.Add(RoleNames.Analyst);
        }

        return globalRoles;
    }

    private async Task SetExclusiveGlobalRole(string? globalRoleNameToSet, ApplicationUser user)
    {
        var existingRoleNames = await identityUserManager.GetRolesAsync(user) ?? [];

        if (globalRoleNameToSet == null)
        {
            await identityUserManager.RemoveFromRolesAsync(user, existingRoleNames);
            return;
        }

        if (!existingRoleNames.Contains(globalRoleNameToSet))
        {
            await identityUserManager.AddToRoleAsync(user, globalRoleNameToSet);
        }

        var rolesToRemove = existingRoleNames.Where(roleName => roleName != globalRoleNameToSet).ToList();

        if (rolesToRemove.Count > 0)
        {
            await identityUserManager.RemoveFromRolesAsync(user, rolesToRemove);
        }
    }
}

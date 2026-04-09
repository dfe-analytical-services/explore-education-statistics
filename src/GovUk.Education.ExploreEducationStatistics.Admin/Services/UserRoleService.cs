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
using GovUk.Education.ExploreEducationStatistics.Content.Model;
using GovUk.Education.ExploreEducationStatistics.Content.Model.Database;
using GovUk.Education.ExploreEducationStatistics.Content.Model.Queries;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using static GovUk.Education.ExploreEducationStatistics.Admin.Models.GlobalRoles;
using static GovUk.Education.ExploreEducationStatistics.Admin.Validators.ValidationErrorMessages;
using static GovUk.Education.ExploreEducationStatistics.Admin.Validators.ValidationUtils;

namespace GovUk.Education.ExploreEducationStatistics.Admin.Services;

public class UserRoleService(
    UsersAndRolesDbContext usersAndRolesDbContext,
    ContentDbContext contentDbContext,
    IPersistenceHelper<ContentDbContext> contentPersistenceHelper,
    IPersistenceHelper<UsersAndRolesDbContext> usersAndRolesPersistenceHelper,
    IUserResourceRoleNotificationService userResourceRoleNotificationService,
    IUserService userService,
    IUserPublicationRoleRepository userPublicationRoleRepository,
    IUserPrereleaseRoleRepository userPrereleaseRoleRepository,
    IUserRepository userRepository,
    UserManager<ApplicationUser> identityUserManager
) : IUserRoleService
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

    public async Task<Either<ActionResult, Dictionary<string, List<string>>>> GetAllResourceRoles() =>
        await userService
            .CheckCanManageAllUsers()
            .OnSuccess(_ =>
            {
                // This will be changed when we finally remove these OLD permissions systems roles
                // from the enum in EES-6215
                HashSet<string> publicationRolesNamesToFilter =
                [
                    nameof(PublicationRole.Allower),
                    nameof(PublicationRole.Owner),
                ];

                return new Dictionary<string, List<string>>
                {
                    {
                        "Publication",
                        Enum.GetNames(typeof(PublicationRole))
                            .Where(name => !publicationRolesNamesToFilter.Contains(name))
                            .OrderBy(name => name)
                            .ToList()
                    },
                    { "Release", [Enum.GetName(ReleaseRole.PrereleaseViewer)] },
                };
            });

    public async Task<Either<ActionResult, List<UserPublicationRoleViewModel>>> GetPublicationRolesForUser(
        Guid userId
    ) =>
        await userService
            .CheckCanManageAllUsers()
            .OnSuccess(_ => FindActiveUser(userId))
            .OnSuccess(async () =>
                await userPublicationRoleRepository
                    .Query()
                    .WhereForUser(userId)
                    .Include(upr => upr.User)
                    .Include(upr => upr.Publication)
                    .OrderBy(upr => upr.Publication.Title)
                    .Select(upr => new UserPublicationRoleViewModel
                    {
                        Id = upr.Id,
                        Publication = upr.Publication.Title,
                        Role = upr.Role,
                        UserName = upr.User.DisplayName,
                        Email = upr.User.Email,
                    })
                    .ToListAsync()
            );

    public async Task<Either<ActionResult, List<UserPublicationRoleViewModel>>> GetPublicationRolesForPublication(
        Guid publicationId
    ) =>
        await contentPersistenceHelper
            .CheckEntityExists<Publication>(publicationId)
            .OnSuccess(userService.CheckCanViewPublication)
            .OnSuccess(async () => (
                    await userPublicationRoleRepository
                        .Query()
                        .WhereForPublication(publicationId)
                        .Include(upr => upr.User)
                        .Include(upr => upr.Publication)
                        .Select(upr => new UserPublicationRoleViewModel
                        {
                            Id = upr.Id,
                            Publication = upr.Publication.Title,
                            Role = upr.Role,
                            UserName = upr.User.DisplayName,
                            Email = upr.User.Email,
                        })
                        .ToListAsync()
                ).OrderBy(upr => upr.UserName).ToList());

    public async Task<Either<ActionResult, Unit>> AddPublicationRole(
        Guid userId,
        Guid publicationId,
        PublicationRole role
    ) =>
        await userService
            .CheckCanManageAllUsers()
            .OnSuccess(async () =>
            {
                return await usersAndRolesPersistenceHelper
                    .CheckEntityExists<ApplicationUser, string>(userId.ToString())
                    .OnSuccessCombineWith(_ => contentPersistenceHelper.CheckEntityExists<Publication>(publicationId))
                    .OnSuccessDo(_ => ValidatePublicationRoleCanBeAdded(userId, publicationId, role))
                    .OnSuccessVoid(async tuple =>
                    {
                        var (user, publication) = tuple;

                        await contentDbContext.RequireTransaction(async () =>
                        {
                            var createdUserPublicationRole = await userPublicationRoleRepository.Create(
                                userId: userId,
                                publicationId: publication.Id,
                                role: role,
                                createdById: userService.GetUserId()
                            );

                            await UpgradeToGlobalRoleIfRequired(AssociatedGlobalRoleNameForPublicationRole, user);

                            await userResourceRoleNotificationService.NotifyUserOfNewPublicationRole(
                                createdUserPublicationRole!.Id
                            );
                        });
                    });
            });

    public async Task<Either<ActionResult, Unit>> RemoveUserPublicationRole(Guid userPublicationRoleId) =>
        await userService
            .CheckCanManageAllUsers()
            .OnSuccess(() => FindUserPublicationRole(userPublicationRoleId))
            .OnSuccessDo(async userPublicationRole =>
            {
                var removed = await userPublicationRoleRepository.RemoveById(userPublicationRole.Id);

                if (!removed)
                {
                    throw new InvalidOperationException(
                        $"Failed to remove User Publication Role with ID {userPublicationRole.Id}"
                    );
                }
            })
            .OnSuccessVoid(async userPublicationRole =>
                await usersAndRolesPersistenceHelper
                    .CheckEntityExists<ApplicationUser, string>(userPublicationRole.UserId.ToString())
                    .OnSuccessDo(user =>
                        DowngradeFromGlobalRoleIfRequired(user, AssociatedGlobalRoleNameForPublicationRole)
                    )
            );

    public async Task<Either<ActionResult, Unit>> RemoveAllUserResourceRoles(Guid userId) =>
        await userService
            .CheckCanManageAllUsers()
            .OnSuccess(async _ =>
            {
                return await FindActiveUser(userId)
                    .OnSuccess(async _ =>
                    {
                        await userPrereleaseRoleRepository.RemoveForUser(userId);
                        await userPublicationRoleRepository.RemoveForUser(userId);

                        await usersAndRolesPersistenceHelper
                            .CheckEntityExists<ApplicationUser, string>(userId.ToString())
                            .OnSuccessDo(async user =>
                            {
                                var existingRoleNames = await identityUserManager.GetRolesAsync(user) ?? [];

                                await identityUserManager.RemoveFromRolesAsync(user, existingRoleNames);
                            });

                        return Unit.Instance;
                    });
            });

    internal async Task UpgradeToGlobalRoleIfRequired(string globalRoleNameToSet, ApplicationUser user)
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

    private async Task DowngradeFromGlobalRoleIfRequired(ApplicationUser user, string globalRoleNameToDowngradeFrom)
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

        var userHasAPrereleaseRole = await userPrereleaseRoleRepository.Query().WhereForUser(userId).AnyAsync();

        var userHasAPublicationRole = await userPublicationRoleRepository.Query().WhereForUser(userId).AnyAsync();

        var globalRoles = new List<string>();

        if (userHasAPrereleaseRole)
        {
            globalRoles.Add(AssociatedGlobalRoleNameForPrereleaseRole);
        }

        if (userHasAPublicationRole)
        {
            globalRoles.Add(AssociatedGlobalRoleNameForPublicationRole);
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

    private async Task<Either<ActionResult, Unit>> ValidatePublicationRoleCanBeAdded(
        Guid userId,
        Guid publicationId,
        PublicationRole role
    )
    {
        if (await userPublicationRoleRepository.UserHasRoleOnPublication(userId, publicationId, role))
        {
            return ValidationActionResult(UserAlreadyHasResourceRole);
        }

        return Unit.Instance;
    }

    private async Task<Either<ActionResult, User>> FindActiveUser(Guid userId) =>
        await userRepository.FindActiveUserById(userId) ?? new Either<ActionResult, User>(new NotFoundResult());

    private async Task<Either<ActionResult, UserPublicationRole>> FindUserPublicationRole(Guid userPublicationRoleId) =>
        await userPublicationRoleRepository.GetById(userPublicationRoleId)
        ?? new Either<ActionResult, UserPublicationRole>(new NotFoundResult());

    private static string AssociatedGlobalRoleNameForPrereleaseRole => RoleNames.PrereleaseUser;

    private static string AssociatedGlobalRoleNameForPublicationRole => RoleNames.Analyst;
}

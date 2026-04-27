#nullable enable
using GovUk.Education.ExploreEducationStatistics.Admin.Controllers.Api.RequestModels;
using GovUk.Education.ExploreEducationStatistics.Admin.Database;
using GovUk.Education.ExploreEducationStatistics.Admin.Models;
using GovUk.Education.ExploreEducationStatistics.Admin.Services.Enums;
using GovUk.Education.ExploreEducationStatistics.Admin.Services.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Admin.Services.Interfaces.Security;
using GovUk.Education.ExploreEducationStatistics.Admin.ViewModels;
using GovUk.Education.ExploreEducationStatistics.Common.Extensions;
using GovUk.Education.ExploreEducationStatistics.Common.Model;
using GovUk.Education.ExploreEducationStatistics.Common.Services.Interfaces.Security;
using GovUk.Education.ExploreEducationStatistics.Common.Utils;
using GovUk.Education.ExploreEducationStatistics.Content.Model;
using GovUk.Education.ExploreEducationStatistics.Content.Model.Database;
using GovUk.Education.ExploreEducationStatistics.Content.Model.Extensions;
using GovUk.Education.ExploreEducationStatistics.Content.Model.Predicates;
using GovUk.Education.ExploreEducationStatistics.Content.Model.Queries;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using static GovUk.Education.ExploreEducationStatistics.Admin.Models.GlobalRoles;
using static GovUk.Education.ExploreEducationStatistics.Admin.Services.UserPublicationRoleRepository;
using static GovUk.Education.ExploreEducationStatistics.Admin.Validators.ValidationErrorMessages;
using static GovUk.Education.ExploreEducationStatistics.Admin.Validators.ValidationUtils;

namespace GovUk.Education.ExploreEducationStatistics.Admin.Services;

public class UserManagementService(
    UsersAndRolesDbContext usersAndRolesDbContext,
    ContentDbContext contentDbContext,
    IPersistenceHelper<UsersAndRolesDbContext> usersAndRolesPersistenceHelper,
    IUserRoleService userRoleService,
    IUserRepository userRepository,
    IUserService userService,
    IUserPreReleaseRoleRepository userPreReleaseRoleRepository,
    IUserPublicationRoleRepository userPublicationRoleRepository,
    IUserResourceRoleNotificationService userResourceRoleNotificationService,
    IPreReleaseUserService preReleaseUserService,
    UserManager<ApplicationUser> userManager
) : IUserManagementService
{
    public async Task<Either<ActionResult, List<UserViewModel>>> ListAllUsers()
    {
        return await userService
            .CheckCanManageAllUsers()
            .OnSuccess(async () =>
            {
                var roles = await usersAndRolesDbContext.Roles.ToListAsync();
                var preReleaseRole = roles.Single(role => role.Name == RoleNames.PrereleaseUser);
                var users = usersAndRolesDbContext.Users;
                var userRoles = usersAndRolesDbContext.UserRoles;
                var nonPreReleaseUsers = users.Where(user =>
                    !userRoles.Any(userRole => userRole.UserId == user.Id && userRole.RoleId == preReleaseRole.Id)
                );
                var usersAndRoles = await nonPreReleaseUsers
                    .Select(user => new
                    {
                        Id = Guid.Parse(user.Id),
                        Name = user.FirstName + " " + user.LastName,
                        user.Email,
                        Role = userRoles
                            .Where(userRole => userRole.UserId == user.Id)
                            .Select(userRole => userRole.RoleId)
                            .FirstOrDefault(),
                    })
                    .ToListAsync();

                return usersAndRoles
                    .Select(userAndRole => new UserViewModel
                    {
                        Id = userAndRole.Id,
                        Name = userAndRole.Name,
                        Email = userAndRole.Email,
                        Role = roles.SingleOrDefault(role => role.Id == userAndRole.Role)?.Name,
                    })
                    .OrderBy(userAndRole => userAndRole.Name)
                    .ToList();
            });
    }

    public async Task<Either<ActionResult, List<RoleViewModel>>> ListRoles()
    {
        return await userService
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
    }

    public async Task<Either<ActionResult, UserViewModel>> GetUser(Guid id)
    {
        return await userService
            .CheckCanManageAllUsers()
            .OnSuccess(async () =>
            {
                return await usersAndRolesPersistenceHelper
                    .CheckEntityExists<ApplicationUser, string>(id.ToString())
                    .OnSuccess(async user =>
                    {
                        return await userRoleService
                            .GetGlobalRolesForUser(user.Id)
                            .OnSuccessCombineWith(_ => userRoleService.GetPublicationRolesForUser(id))
                            .OnSuccessCombineWith(_ => preReleaseUserService.GetPreReleaseRolesForUser(id))
                            .OnSuccess(tuple =>
                            {
                                var (globalRoles, publicationRoles, preReleaseRoles) = tuple;

                                // Currently we only allow a user to have a maximum of one global role,
                                // and potentially no global role at all if other permissions in the system
                                // have been removed.
                                var globalRole = globalRoles.FirstOrDefault();

                                return new UserViewModel
                                {
                                    Id = id,
                                    Name = user.FirstName + " " + user.LastName,
                                    Email = user.Email!,
                                    Role = globalRole?.Id,
                                    UserPublicationRoles = publicationRoles,
                                    UserPreReleaseRoles = preReleaseRoles,
                                };
                            });
                    });
            });
    }

    public async Task<Either<ActionResult, List<PendingInviteViewModel>>> ListPendingInvites()
    {
        return await userService
            .CheckCanManageAllUsers()
            .OnSuccess(async () =>
            {
                var pendingUserInvites = await contentDbContext
                    .Users.AsNoTracking()
                    .WhereInvitePending()
                    .Select(u => new
                    {
                        UserId = u.Id,
                        u.Email,
                        u.Role,
                    })
                    .OrderBy(u => u.Email)
                    .ToListAsync();

                return await pendingUserInvites
                    .ToAsyncEnumerable()
                    .Select(
                        async (pendingUserInvite, _, cancellationToken) =>
                        {
                            var userPreReleaseRoles = await userPreReleaseRoleRepository
                                .Query(ResourceRoleFilter.PendingOnly)
                                .WhereForUser(pendingUserInvite.UserId)
                                .Select(urr => new UserPreReleaseRoleViewModel
                                {
                                    Id = urr.Id,
                                    Publication = urr.ReleaseVersion.Release.Publication.Title,
                                    Release = urr.ReleaseVersion.Release.Title,
                                })
                                .ToListAsync(cancellationToken);

                            var userPublicationRoles = await userPublicationRoleRepository
                                .Query(ResourceRoleFilter.PendingOnly)
                                .WhereForUser(pendingUserInvite.UserId)
                                .Select(upr => new UserPublicationRoleViewModel
                                {
                                    Id = upr.Id,
                                    Publication = upr.Publication.Title,
                                    Role = upr.Role,
                                })
                                .ToListAsync(cancellationToken);

                            return new PendingInviteViewModel
                            {
                                Email = pendingUserInvite.Email,
                                Role = pendingUserInvite.Role!.Name!,
                                UserPublicationRoles = userPublicationRoles,
                                UserPreReleaseRoles = userPreReleaseRoles,
                            };
                        }
                    )
                    .ToListAsync();
            });
    }

    public async Task<Either<ActionResult, User>> InviteUser(UserInviteCreateRequest request)
    {
        var sanitisedEmail = request.Email.Trim().ToLower();
        var roleId = request.RoleId;

        return await userService
            .CheckCanManageAllUsers()
            .OnSuccess(() => ValidateActiveUserDoesNotExist(sanitisedEmail))
            .OnSuccess<ActionResult, Unit, User>(async () =>
            {
                var role = await usersAndRolesDbContext.Roles.AsNoTracking().SingleOrDefaultAsync(r => r.Id == roleId);

                if (role is null)
                {
                    return ValidationActionResult(InvalidUserRole);
                }

                var user = await userRepository.CreateOrUpdate(
                    email: sanitisedEmail,
                    roleId: roleId,
                    createdById: userService.GetUserId(),
                    createdDate: request.CreatedDate
                );

                // Clear out any pre-existing Release Roles or Publication Roles prior to adding
                // new ones.
                await userPreReleaseRoleRepository.RemoveForUser(user.Id);
                await userPublicationRoleRepository.RemoveForUser(user.Id);

                foreach (var userPreReleaseRole in request.UserPreReleaseRoles)
                {
                    var latestReleaseVersion = await contentDbContext
                        .ReleaseVersions.LatestReleaseVersion(releaseId: userPreReleaseRole.ReleaseId)
                        .SingleAsync();

                    await userPreReleaseRoleRepository.Create(
                        userId: user.Id,
                        releaseVersionId: latestReleaseVersion!.Id,
                        createdById: userService.GetUserId(),
                        createdDate: request.CreatedDate?.UtcDateTime
                    );
                }

                var userPublicationRolesToCreate = request
                    .UserPublicationRoles.Select(userPublicationRole => new UserPublicationRoleCreateDto(
                        UserId: user.Id,
                        PublicationId: userPublicationRole.PublicationId,
                        Role: userPublicationRole.PublicationRole,
                        CreatedDate: request.CreatedDate?.UtcDateTime ?? DateTime.UtcNow,
                        CreatedById: userService.GetUserId()
                    ))
                    .ToHashSet();

                await userPublicationRoleRepository.CreateManyIfNotExists(userPublicationRolesToCreate);

                await userResourceRoleNotificationService.NotifyUserOfInvite(user.Id);

                return user;
            });
    }

    public async Task<Either<ActionResult, Unit>> CancelInvite(string email)
    {
        return await userService
            .CheckCanManageAllUsers()
            .OnSuccess(async () => await GetPendingUserInvite(email))
            .OnSuccessVoid(async invitedUser =>
            {
                await contentDbContext.RequireTransaction(async () =>
                {
                    await userPreReleaseRoleRepository.RemoveForUser(invitedUser.Id);
                    await userPublicationRoleRepository.RemoveForUser(invitedUser.Id);

                    await userRepository.SoftDeleteUser(invitedUser.Id, userService.GetUserId());
                });
            });
    }

    public async Task<Either<ActionResult, Unit>> UpdateUser(string userId, string roleId)
    {
        return await userService
            .CheckCanManageAllUsers()
            .OnSuccess(() => userRoleService.SetGlobalRoleForUser(userId, roleId));
    }

    public async Task<Either<ActionResult, Unit>> DeleteUser(string email)
    {
        return await userService
            .CheckCanManageAllUsers()
            .OnSuccess(async () => await GetActiveUser(email))
            .OnSuccessCombineWith(async _ => await GetIdentityUser(email))
            .OnSuccessVoid(async tuple =>
            {
                var (activeInternalUser, identityUser) = tuple;

                await contentDbContext.RequireTransaction(async () =>
                {
                    await userManager.DeleteAsync(identityUser);

                    await userPreReleaseRoleRepository.RemoveForUser(activeInternalUser.Id);
                    await userPublicationRoleRepository.RemoveForUser(activeInternalUser.Id);

                    await userRepository.SoftDeleteUser(activeInternalUser.Id, userService.GetUserId());
                });
            });
    }

    private async Task<Either<ActionResult, User>> GetActiveUser(string email) =>
        await userRepository.FindActiveUserByEmail(email) ?? new Either<ActionResult, User>(new NotFoundResult());

    private async Task<Either<ActionResult, ApplicationUser>> GetIdentityUser(string email) =>
        await usersAndRolesDbContext.Users.SingleOrNotFoundAsync(user => user.Email == email);

    private async Task<Either<ActionResult, Unit>> ValidateActiveUserDoesNotExist(string email) =>
        await userRepository.FindActiveUserByEmail(email) is not null
            ? ValidationActionResult(UserAlreadyExists)
            : Unit.Instance;

    private async Task<Either<ActionResult, User>> GetPendingUserInvite(string email) =>
        await userRepository.FindPendingUserInviteByEmail(email)
        ?? new Either<ActionResult, User>(ValidationActionResult(InviteNotFound));
}

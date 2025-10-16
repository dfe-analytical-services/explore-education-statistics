#nullable enable
using AngleSharp.Dom;
using GovUk.Education.ExploreEducationStatistics.Admin.Database;
using GovUk.Education.ExploreEducationStatistics.Admin.Models;
using GovUk.Education.ExploreEducationStatistics.Admin.Services.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Admin.Services.Interfaces.Security;
using GovUk.Education.ExploreEducationStatistics.Admin.ViewModels;
using GovUk.Education.ExploreEducationStatistics.Common.Extensions;
using GovUk.Education.ExploreEducationStatistics.Common.Model;
using GovUk.Education.ExploreEducationStatistics.Common.Services.Interfaces.Security;
using GovUk.Education.ExploreEducationStatistics.Common.Utils;
using GovUk.Education.ExploreEducationStatistics.Common.ViewModels;
using GovUk.Education.ExploreEducationStatistics.Content.Model;
using GovUk.Education.ExploreEducationStatistics.Content.Model.Database;
using GovUk.Education.ExploreEducationStatistics.Content.Model.Extensions;
using GovUk.Education.ExploreEducationStatistics.Content.Model.Predicates;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using static GovUk.Education.ExploreEducationStatistics.Admin.Models.GlobalRoles;
using static GovUk.Education.ExploreEducationStatistics.Admin.Validators.ValidationErrorMessages;
using static GovUk.Education.ExploreEducationStatistics.Admin.Validators.ValidationUtils;

namespace GovUk.Education.ExploreEducationStatistics.Admin.Services;

public class UserManagementService(
    UsersAndRolesDbContext usersAndRolesDbContext,
    ContentDbContext contentDbContext,
    IPersistenceHelper<UsersAndRolesDbContext> usersAndRolesPersistenceHelper,
    IEmailTemplateService emailTemplateService,
    IUserRoleService userRoleService,
    IUserRepository userRepository,
    IUserService userService,
    IUserReleaseInviteRepository userReleaseInviteRepository,
    IUserPublicationInviteRepository userPublicationInviteRepository,
    IUserReleaseRoleRepository userReleaseRoleRepository,
    IUserPublicationRoleRepository userPublicationRoleRepository,
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
                var prereleaseRole = roles.Single(role => role.Name == RoleNames.PrereleaseUser);
                var users = usersAndRolesDbContext.Users;
                var userRoles = usersAndRolesDbContext.UserRoles;
                var nonPrereleaseUsers = users.Where(user =>
                    !userRoles.Any(userRole => userRole.UserId == user.Id && userRole.RoleId == prereleaseRole.Id)
                );
                var usersAndRoles = await nonPrereleaseUsers
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

    public async Task<Either<ActionResult, List<IdTitleViewModel>>> ListReleases()
    {
        return await userService
            .CheckCanManageAllUsers()
            .OnSuccess(async () =>
                await contentDbContext
                    .Releases.Select(r => new IdTitleViewModel
                    {
                        Id = r.Id,
                        Title = $"{r.Publication.Title} - {r.Title}",
                    })
                    .ToListAsync()
            );
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

    public async Task<List<UserViewModel>> ListPreReleaseUsersAsync()
    {
        return await usersAndRolesDbContext
            .Users.AsQueryable()
            .Join(
                usersAndRolesDbContext.UserRoles,
                user => user.Id,
                userRole => userRole.UserId,
                (user, userRole) => new { user, userRoleId = userRole.RoleId }
            )
            .Join(
                usersAndRolesDbContext.Roles,
                prev => prev.userRoleId,
                role => role.Id,
                (prev, role) =>
                    new UserViewModel
                    {
                        Id = Guid.Parse(prev.user.Id),
                        Name = prev.user.FirstName + " " + prev.user.LastName,
                        Email = prev.user.Email,
                        Role = role.Name,
                    }
            )
            .OrderBy(x => x.Name)
            .Where(u => u.Role == Role.PrereleaseUser.GetEnumLabel())
            .ToListAsync();
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
                            .GetGlobalRoles(user.Id)
                            .OnSuccessCombineWith(_ => userRoleService.GetPublicationRolesForUser(id))
                            .OnSuccessCombineWith(_ => userRoleService.GetReleaseRoles(id))
                            .OnSuccess(tuple =>
                            {
                                var (globalRoles, publicationRoles, releaseRoles) = tuple;

                                // Currently we only allow a user to have a maximum of one global role,
                                // and potentially no global role at all if other permissions in the system
                                // have been removed.
                                var globalRole = globalRoles.FirstOrDefault();

                                return new UserViewModel
                                {
                                    Id = id,
                                    Name = user.FirstName + " " + user.LastName,
                                    Email = user.Email,
                                    Role = globalRole?.Id,
                                    UserPublicationRoles = publicationRoles,
                                    UserReleaseRoles = releaseRoles,
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
                    .Users.WhereIsPendingInvite()
                    .Select(u => new { u.Email, u.Role })
                    .OrderBy(u => u.Email)
                    .ToListAsync();

                return await pendingUserInvites
                    .ToAsyncEnumerable()
                    .SelectAwait(async pendingUserInvite =>
                    {
                        var userReleaseInvites = await contentDbContext
                            .UserReleaseInvites.Include(uri => uri.ReleaseVersion)
                            .ThenInclude(rv => rv.Release)
                            .ThenInclude(r => r.Publication)
                            .Where(uri => uri.Email.ToLower().Equals(pendingUserInvite.Email.ToLower()))
                            .ToListAsync();

                        var userReleaseRoles = userReleaseInvites
                            .Select(userReleaseInvite => new UserReleaseRoleViewModel
                            {
                                Id = userReleaseInvite.Id,
                                Publication = userReleaseInvite.ReleaseVersion.Release.Publication.Title,
                                Release = userReleaseInvite.ReleaseVersion.Release.Title,
                                Role = userReleaseInvite.Role,
                            })
                            .ToList();

                        var userPublicationInvites = await contentDbContext
                            .UserPublicationInvites.Include(upi => upi.Publication)
                            .Where(upi => upi.Email.ToLower().Equals(pendingUserInvite.Email.ToLower()))
                            .ToListAsync();

                        var userPublicationRoles = userPublicationInvites
                            .Select(userPublicationInvite => new UserPublicationRoleViewModel
                            {
                                Id = userPublicationInvite.Id,
                                Publication = userPublicationInvite.Publication.Title,
                                Role = userPublicationInvite.Role,
                            })
                            .ToList();

                        return new PendingInviteViewModel
                        {
                            Email = pendingUserInvite.Email,
                            Role = pendingUserInvite.Role.Name,
                            UserPublicationRoles = userPublicationRoles,
                            UserReleaseRoles = userReleaseRoles,
                        };
                    })
                    .ToListAsync();
            });
    }

    public async Task<Either<ActionResult, User>> InviteUser(UserInviteCreateRequest request)
    {
        var email = request.Email;
        var sanitisedEmail = email.Trim().ToLower();
        var roleId = request.RoleId;

        return await userService
            .CheckCanManageAllUsers()
            .OnSuccess(() => ValidateIdentityUserDoesNotExist(email))
            .OnSuccess<ActionResult, Unit, User>(async () =>
            {
                var role = await usersAndRolesDbContext.Roles.FirstOrDefaultAsync(r => r.Id == roleId);

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

                // Clear out any pre-existing Release or Publication invites prior to adding
                // new ones.
                await userReleaseInviteRepository.RemoveByUserEmail(sanitisedEmail);
                await userPublicationInviteRepository.RemoveByUserEmail(sanitisedEmail);

                foreach (var userReleaseRole in request.UserReleaseRoles)
                {
                    var latestReleaseVersion = await contentDbContext
                        .ReleaseVersions.LatestReleaseVersion(releaseId: userReleaseRole.ReleaseId)
                        .SingleAsync();

                    await userReleaseInviteRepository.Create(
                        releaseVersionId: latestReleaseVersion!.Id,
                        email: sanitisedEmail,
                        releaseRole: userReleaseRole.ReleaseRole,
                        emailSent: true,
                        createdById: userService.GetUserId(),
                        request.CreatedDate?.UtcDateTime
                    );
                }

                await userPublicationInviteRepository.CreateManyIfNotExists(
                    request.UserPublicationRoles,
                    sanitisedEmail,
                    userService.GetUserId(),
                    request.CreatedDate?.UtcDateTime
                );

                return user;
            })
            .OnSuccess(async user =>
            {
                var userReleaseInvites = await contentDbContext
                    .UserReleaseInvites.Include(uri => uri.ReleaseVersion)
                    .ThenInclude(rv => rv.Release)
                    .ThenInclude(r => r.Publication)
                    .Where(uri => uri.Email.ToLower() == sanitisedEmail)
                    .ToListAsync();

                var userPublicationInvites = await contentDbContext
                    .UserPublicationInvites.Include(upi => upi.Publication)
                    .Where(upi => upi.Email.ToLower() == sanitisedEmail)
                    .ToListAsync();

                return emailTemplateService
                    .SendInviteEmail(sanitisedEmail, userReleaseInvites, userPublicationInvites)
                    .OnSuccess(() => user);
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
                    await userReleaseInviteRepository.RemoveByUserEmail(email);
                    await userPublicationInviteRepository.RemoveByUserEmail(email);

                    await userRepository.SoftDeleteUser(invitedUser, userService.GetUserId());
                });
            });
    }

    public async Task<Either<ActionResult, Unit>> UpdateUser(string userId, string roleId)
    {
        return await userService
            .CheckCanManageAllUsers()
            .OnSuccess(() => userRoleService.SetGlobalRole(userId, roleId));
    }

    public async Task<Either<ActionResult, Unit>> DeleteUser(string email)
    {
        return await userService
            .CheckCanManageAllUsers()
            .OnSuccess(async () => await GetActiveUser(email))
            .OnSuccessCombineWith(async activeInternalUser => await GetIdentityUser(email))
            .OnSuccessVoid(async tuple =>
            {
                var (activeInternalUser, identityUser) = tuple;

                await contentDbContext.RequireTransaction(async () =>
                {
                    await userManager.DeleteAsync(identityUser);

                    await userPublicationInviteRepository.RemoveByUserEmail(email);
                    await userReleaseInviteRepository.RemoveByUserEmail(email);

                    await userReleaseRoleRepository.RemoveForUser(activeInternalUser.Id);
                    await userPublicationRoleRepository.RemoveForUser(activeInternalUser.Id);

                    await userRepository.SoftDeleteUser(activeInternalUser, userService.GetUserId());
                });
            });
    }

    private async Task<Either<ActionResult, User>> GetActiveUser(string email)
    {
        var activeUser = await userRepository.FindActiveUserByEmail(email);

        return activeUser is null ? new NotFoundResult() : activeUser;
    }

    private async Task<Either<ActionResult, ApplicationUser>> GetIdentityUser(string email)
    {
        var identityUser = await usersAndRolesDbContext.Users.SingleOrDefaultAsync(user => user.Email == email);

        return identityUser is null ? new NotFoundResult() : identityUser;
    }

    private async Task<Either<ActionResult, Unit>> ValidateIdentityUserDoesNotExist(string email)
    {
        return await usersAndRolesDbContext.Users.AnyAsync(u => u.Email!.ToLower().Equals(email.ToLower()))
            ? ValidationActionResult(UserAlreadyExists)
            : Unit.Instance;
    }

    private async Task<Either<ActionResult, User>> GetPendingUserInvite(string email)
    {
        var pendingInvite = await userRepository.FindPendingUserInviteByEmail(email);

        return pendingInvite is null ? ValidationActionResult(InviteNotFound) : pendingInvite;
    }
}

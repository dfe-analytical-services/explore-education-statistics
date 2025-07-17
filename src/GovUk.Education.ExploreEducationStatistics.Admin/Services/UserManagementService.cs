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
using GovUk.Education.ExploreEducationStatistics.Common.ViewModels;
using GovUk.Education.ExploreEducationStatistics.Content.Model.Database;
using GovUk.Education.ExploreEducationStatistics.Content.Model.Predicates;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
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
    IUserInviteRepository userInviteRepository,
    IUserReleaseInviteRepository userReleaseInviteRepository,
    IUserPublicationInviteRepository userPublicationInviteRepository,
    IUserReleaseRoleAndInviteManager userReleaseRoleAndInviteManager,
    IUserPublicationRoleAndInviteManager userPublicationRoleAndInviteManager,
    UserManager<ApplicationUser> userManager) : IUserManagementService
{
    public async Task<Either<ActionResult, List<UserViewModel>>> ListAllUsers()
    {
        return await userService
            .CheckCanManageAllUsers()
            .OnSuccess(async () =>
            {
                var roles = await usersAndRolesDbContext.Roles.ToListAsync();
                var prereleaseRole = roles
                    .Single(role => role.Name == RoleNames.PrereleaseUser);
                var users = usersAndRolesDbContext.Users;
                var userRoles = usersAndRolesDbContext.UserRoles;
                var nonPrereleaseUsers = users
                    .Where(user => !userRoles.Any(userRole =>
                        userRole.UserId == user.Id && userRole.RoleId == prereleaseRole.Id));
                var usersAndRoles = await nonPrereleaseUsers
                    .Select(user => new
                    {
                        Id = Guid.Parse(user.Id),
                        Name = user.FirstName + " " + user.LastName,
                        user.Email,
                        Role = userRoles
                            .Where(userRole => userRole.UserId == user.Id)
                            .Select(userRole => userRole.RoleId)
                            .FirstOrDefault()
                    })
                    .ToListAsync();

                return usersAndRoles
                    .Select(userAndRole => new UserViewModel
                    {
                        Id = userAndRole.Id,
                        Name = userAndRole.Name,
                        Email = userAndRole.Email,
                        Role = roles.SingleOrDefault(role => role.Id == userAndRole.Role)?.Name
                    })
                    .OrderBy(userAndRole => userAndRole.Name)
                    .ToList();
            });
    }

    public async Task<Either<ActionResult, List<IdTitleViewModel>>> ListReleases()
    {
        return await userService
            .CheckCanManageAllUsers()
            .OnSuccess(async () => await contentDbContext.Releases
                .Select(r => new IdTitleViewModel
                {
                    Id = r.Id,
                    Title = $"{r.Publication.Title} - {r.Title}"
                })
                .ToListAsync());
    }

    public async Task<Either<ActionResult, List<RoleViewModel>>> ListRoles()
    {
        return await userService
            .CheckCanManageAllUsers()
            .OnSuccess(async () =>
            {
                return await usersAndRolesDbContext.Roles
                    .AsQueryable()
                    .Select(r => new RoleViewModel
                    {
                        Id = r.Id,
                        Name = r.Name,
                        NormalizedName = r.NormalizedName
                    })
                    .OrderBy(x => x.Name)
                    .ToListAsync();
            });
    }

    public async Task<List<UserViewModel>> ListPreReleaseUsersAsync()
    {
        return await usersAndRolesDbContext.Users
            .AsQueryable()
            .Join(
                usersAndRolesDbContext.UserRoles,
                user => user.Id,
                userRole => userRole.UserId,
                (user, userRole) => new
                {
                    user,
                    userRoleId = userRole.RoleId
                }
            )
            .Join(
                usersAndRolesDbContext.Roles,
                prev => prev.userRoleId,
                role => role.Id,
                (prev, role) => new UserViewModel
                {
                    Id = Guid.Parse(prev.user.Id),
                    Name = prev.user.FirstName + " " + prev.user.LastName,
                    Email = prev.user.Email,
                    Role = role.Name
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
                        return await userRoleService.GetGlobalRoles(user.Id)
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
                                    UserReleaseRoles = releaseRoles
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
                    var pendingInvites = await usersAndRolesDbContext.UserInvites
                        .AsQueryable()
                        .Where(ui => !ui.Accepted)
                        .OrderBy(ui => ui.Email)
                        .Include(ui => ui.Role)
                        .ToListAsync();

                    return await pendingInvites
                        .ToAsyncEnumerable()
                        .SelectAwait(async invite =>
                        {
                            var userReleaseInvites = await contentDbContext
                                .UserReleaseInvites
                                .Include(uri => uri.ReleaseVersion)
                                .ThenInclude(rv => rv.Release)
                                .ThenInclude(r => r.Publication)
                                .Where(uri => uri.Email.ToLower().Equals(invite.Email.ToLower()))
                                .ToListAsync();

                            var userReleaseRoles = userReleaseInvites
                                .Select(userReleaseInvite =>
                                    new UserReleaseRoleViewModel
                                    {
                                        Id = userReleaseInvite.Id,
                                        Publication = userReleaseInvite.ReleaseVersion.Release.Publication.Title,
                                        Release = userReleaseInvite.ReleaseVersion.Release.Title,
                                        Role = userReleaseInvite.Role,
                                    }
                                ).ToList();

                            var userPublicationInvites = await contentDbContext
                                .UserPublicationInvites
                                .Include(upi => upi.Publication)
                                .Where(upi => upi.Email.ToLower().Equals(invite.Email.ToLower()))
                                .ToListAsync();

                            var userPublicationRoles = userPublicationInvites
                                .Select(userPublicationInvite =>
                                    new UserPublicationRoleViewModel
                                    {
                                        Id = userPublicationInvite.Id,
                                        Publication = userPublicationInvite.Publication.Title,
                                        Role = userPublicationInvite.Role,
                                    }
                                ).ToList();

                            return new PendingInviteViewModel
                            {
                                Email = invite.Email,
                                Role = invite.Role.Name,
                                UserPublicationRoles = userPublicationRoles,
                                UserReleaseRoles = userReleaseRoles,
                            };
                        }).ToListAsync();
                }
            );
    }

    public async Task<Either<ActionResult, UserInvite>> InviteUser(UserInviteCreateRequest request)
    {
        var email = request.Email;
        var sanitisedEmail = email.Trim().ToLower();
        var roleId = request.RoleId;

        return await userService
            .CheckCanManageAllUsers()
            .OnSuccess(() => ValidateUserDoesNotExist(email))
            .OnSuccess<ActionResult, Unit, UserInvite>(async () =>
            {
                var role = await usersAndRolesDbContext.Roles
                    .AsQueryable()
                    .FirstOrDefaultAsync(r => r.Id == roleId);

                if (role == null)
                {
                    return ValidationActionResult(InvalidUserRole);
                }

                var userInvite = await userInviteRepository.CreateOrUpdate(
                    email: sanitisedEmail,
                    roleId: roleId,
                    createdById: userService.GetUserId(),
                    request.CreatedDate);

                // Clear out any pre-existing Release or Publication invites prior to adding
                // new ones.
                var existingReleaseInvites = await userReleaseInviteRepository.ListByEmail(sanitisedEmail);
                var existingPublicationInvites = await userPublicationInviteRepository.ListByEmail(sanitisedEmail);

                await userReleaseInviteRepository.RemoveMany(existingReleaseInvites);
                await userPublicationInviteRepository.RemoveMany(existingPublicationInvites);

                foreach (var userReleaseRole in request.UserReleaseRoles)
                {
                    var latestReleaseVersion = await contentDbContext.ReleaseVersions
                        .LatestReleaseVersion(releaseId: userReleaseRole.ReleaseId)
                        .SingleAsync();

                    await userReleaseInviteRepository.Create(
                        releaseVersionId: latestReleaseVersion!.Id,
                        email: sanitisedEmail,
                        releaseRole: userReleaseRole.ReleaseRole,
                        emailSent: true,
                        createdById: userService.GetUserId(),
                        request.CreatedDate);
                }

                await userPublicationInviteRepository.CreateManyIfNotExists(
                    request.UserPublicationRoles,
                    sanitisedEmail,
                    userService.GetUserId(),
                    request.CreatedDate);

                return userInvite;
            })
            .OnSuccess(async userInvite =>
            {
                var userReleaseInvites = await contentDbContext
                    .UserReleaseInvites
                    .Include(uri => uri.ReleaseVersion)
                    .ThenInclude(rv => rv.Release)
                    .ThenInclude(r => r.Publication)
                    .Where(uri => uri.Email.ToLower() == sanitisedEmail)
                    .ToListAsync();

                var userPublicationInvites = await contentDbContext
                    .UserPublicationInvites
                    .Include(upi => upi.Publication)
                    .Where(upi => upi.Email.ToLower() == sanitisedEmail)
                    .ToListAsync();

                return emailTemplateService
                    .SendInviteEmail(sanitisedEmail, userReleaseInvites, userPublicationInvites)
                    .OnSuccess(() => userInvite);
            });
    }

    public async Task<Either<ActionResult, Unit>> CancelInvite(string email)
    {
        return await userService
            .CheckCanManageAllUsers()
            .OnSuccess(async () => await GetPendingUserInvite(email))
            .OnSuccessVoid(async invite =>
            {
                await contentDbContext.RequireTransaction(async () =>
                {
                    await userReleaseInviteRepository.RemoveByUser(email);
                    await userPublicationInviteRepository.RemoveByUser(email);

                    usersAndRolesDbContext.UserInvites.Remove(invite);
                    await usersAndRolesDbContext.SaveChangesAsync();
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
            .OnSuccess(async () => await GetIdentityUser(email))
            .OnSuccess(async identityUser =>
            {
                var internalUser = await userRepository.FindByEmail(email);

                return (identityUser, internalUser);
            })
            .OnSuccessDo(tuple =>
            {
                return tuple.identityUser is null && tuple.internalUser is null 
                ? (Either<ActionResult, Unit>)new NotFoundResult() 
                : Unit.Instance;
            })
            .OnSuccessVoid(async tuple =>
            {
                var globalInvites = await usersAndRolesDbContext
                    .UserInvites
                    .Where(invite => invite.Email.ToLower() == email.ToLower())
                    .ToListAsync();

                await contentDbContext.RequireTransaction(async () =>
                {
                    // Delete the Identity Framework user, if found.
                    if (tuple.identityUser is not null)
                    {
                        await userManager.DeleteAsync(tuple.identityUser);
                    }

                    // Delete any invites that they may have had.
                    usersAndRolesDbContext.UserInvites.RemoveRange(globalInvites);

                    if (tuple.internalUser is not null)
                    {
                        await userReleaseRoleAndInviteManager.RemoveAllRolesAndInvitesForUser(tuple.internalUser.Id);
                        await userPublicationRoleAndInviteManager.RemoveAllRolesAndInvitesForUser(tuple.internalUser.Id);
                    }

                    if (tuple.internalUser is { SoftDeleted: null })
                    {
                        tuple.internalUser.SoftDeleted = DateTime.UtcNow;
                        tuple.internalUser.DeletedById = userService.GetUserId();
                    }

                    await contentDbContext.SaveChangesAsync();
                    await usersAndRolesDbContext.SaveChangesAsync();
                });
            });
    }

    private async Task<ApplicationUser?> GetIdentityUser(string email)
    {
        return await usersAndRolesDbContext
            .Users
            .SingleOrDefaultAsync(user => user.Email.ToLower() == email.ToLower());
    }

    private async Task<Either<ActionResult, Unit>> ValidateUserDoesNotExist(string email)
    {
        if (await usersAndRolesDbContext.Users
            .AsQueryable()
            .AnyAsync(u => u.Email.ToLower() == email.ToLower()))
        {
            return ValidationActionResult(UserAlreadyExists);
        }

        return Unit.Instance;
    }

    private async Task<Either<ActionResult, UserInvite>> GetPendingUserInvite(string email)
    {
        var invite = await usersAndRolesDbContext.UserInvites
            .FirstOrDefaultAsync(i => i.Email.ToLower() == email.ToLower());

        return invite is null
            ? ValidationActionResult(InviteNotFound)
            : invite.Accepted
            ? ValidationActionResult(InviteAlreadyAccepted)
            : invite;
    }
}

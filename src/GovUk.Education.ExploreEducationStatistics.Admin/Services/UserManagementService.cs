#nullable enable
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
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
using static GovUk.Education.ExploreEducationStatistics.Admin.Models.GlobalRoles;
using static GovUk.Education.ExploreEducationStatistics.Admin.Validators.ValidationUtils;
using static GovUk.Education.ExploreEducationStatistics.Admin.Validators.ValidationErrorMessages;

namespace GovUk.Education.ExploreEducationStatistics.Admin.Services;

public class UserManagementService : IUserManagementService
{
    private readonly UsersAndRolesDbContext _usersAndRolesDbContext;
    private readonly ContentDbContext _contentDbContext;
    private readonly IPersistenceHelper<UsersAndRolesDbContext> _usersAndRolesPersistenceHelper;
    private readonly IEmailTemplateService _emailTemplateService;
    private readonly IUserRoleService _userRoleService;
    private readonly IUserService _userService;
    private readonly IUserInviteRepository _userInviteRepository;
    private readonly IUserReleaseInviteRepository _userReleaseInviteRepository;
    private readonly IUserPublicationInviteRepository _userPublicationInviteRepository;
    private readonly UserManager<ApplicationUser> _userManager;

    public UserManagementService(
        UsersAndRolesDbContext usersAndRolesDbContext,
        ContentDbContext contentDbContext,
        IPersistenceHelper<UsersAndRolesDbContext> usersAndRolesPersistenceHelper,
        IEmailTemplateService emailTemplateService,
        IUserRoleService userRoleService,
        IUserService userService,
        IUserInviteRepository userInviteRepository,
        IUserReleaseInviteRepository userReleaseInviteRepository,
        IUserPublicationInviteRepository userPublicationInviteRepository,
        UserManager<ApplicationUser> userManager)
    {
        _usersAndRolesDbContext = usersAndRolesDbContext;
        _contentDbContext = contentDbContext;
        _usersAndRolesPersistenceHelper = usersAndRolesPersistenceHelper;
        _emailTemplateService = emailTemplateService;
        _userRoleService = userRoleService;
        _userService = userService;
        _userInviteRepository = userInviteRepository;
        _userReleaseInviteRepository = userReleaseInviteRepository;
        _userPublicationInviteRepository = userPublicationInviteRepository;
        _userManager = userManager;
    }

    public async Task<Either<ActionResult, List<UserViewModel>>> ListAllUsers()
    {
        return await _userService
            .CheckCanManageAllUsers()
            .OnSuccess(async () =>
            {
                var roles = await _usersAndRolesDbContext.Roles.ToListAsync();
                var prereleaseRole = roles
                    .Single(role => role.Name == RoleNames.PrereleaseUser);
                var users = _usersAndRolesDbContext.Users;
                var userRoles = _usersAndRolesDbContext.UserRoles;
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
        return await _userService
            .CheckCanManageAllUsers()
            .OnSuccess(async () => await _contentDbContext.Releases
                .Select(r => new IdTitleViewModel
                {
                    Id = r.Id,
                    Title = $"{r.Publication.Title} - {r.Title}"
                })
                .ToListAsync());
    }

    public async Task<Either<ActionResult, List<RoleViewModel>>> ListRoles()
    {
        return await _userService
            .CheckCanManageAllUsers()
            .OnSuccess(async () =>
            {
                return await _usersAndRolesDbContext.Roles
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
        return await _usersAndRolesDbContext.Users
            .AsQueryable()
            .Join(
                _usersAndRolesDbContext.UserRoles,
                user => user.Id,
                userRole => userRole.UserId,
                (user, userRole) => new
                {
                    user,
                    userRoleId = userRole.RoleId
                }
            )
            .Join(
                _usersAndRolesDbContext.Roles,
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
        return await _userService
            .CheckCanManageAllUsers()
            .OnSuccess(async () =>
            {
                return await _usersAndRolesPersistenceHelper
                    .CheckEntityExists<ApplicationUser, string>(id.ToString())
                    .OnSuccess(async user =>
                    {
                        return await _userRoleService.GetGlobalRoles(user.Id)
                            .OnSuccessCombineWith(_ => _userRoleService.GetPublicationRolesForUser(id))
                            .OnSuccessCombineWith(_ => _userRoleService.GetReleaseRoles(id))
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
        return await _userService
            .CheckCanManageAllUsers()
            .OnSuccess(async () =>
                {
                    var pendingInvites = await _usersAndRolesDbContext.UserInvites
                        .AsQueryable()
                        .Where(ui => !ui.Accepted)
                        .OrderBy(ui => ui.Email)
                        .Include(ui => ui.Role)
                        .ToListAsync();

                    return await pendingInvites
                        .ToAsyncEnumerable()
                        .SelectAwait(async invite =>
                        {
                            var userReleaseInvites = await _contentDbContext
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

                            var userPublicationInvites = await _contentDbContext
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

        return await _userService
            .CheckCanManageAllUsers()
            .OnSuccess(() => ValidateUserDoesNotExist(email))
            .OnSuccess<ActionResult, Unit, UserInvite>(async () =>
            {
                var role = await _usersAndRolesDbContext.Roles
                    .AsQueryable()
                    .FirstOrDefaultAsync(r => r.Id == roleId);

                if (role == null)
                {
                    return ValidationActionResult(InvalidUserRole);
                }

                var userInvite = await _userInviteRepository.CreateOrUpdate(
                    email: sanitisedEmail,
                    roleId: roleId,
                    createdById: _userService.GetUserId(),
                    request.CreatedDate);

                // Clear out any pre-existing Release or Publication invites prior to adding
                // new ones.
                var existingReleaseInvites = _contentDbContext
                    .UserReleaseInvites
                    .Where(invite => invite.Email.ToLower() == sanitisedEmail);

                var existingPublicationInvites = _contentDbContext
                    .UserPublicationInvites
                    .Where(invite => invite.Email.ToLower() == sanitisedEmail);

                _contentDbContext.RemoveRange(existingReleaseInvites);
                _contentDbContext.RemoveRange(existingPublicationInvites);

                foreach (var userReleaseRole in request.UserReleaseRoles)
                {
                    var latestReleaseVersion = await _contentDbContext.ReleaseVersions
                        .LatestReleaseVersion(releaseId: userReleaseRole.ReleaseId)
                        .SingleAsync();

                    await _userReleaseInviteRepository.Create(
                        releaseVersionId: latestReleaseVersion!.Id,
                        email: sanitisedEmail,
                        releaseRole: userReleaseRole.ReleaseRole,
                        emailSent: true,
                        createdById: _userService.GetUserId(),
                        request.CreatedDate);
                }

                await _userPublicationInviteRepository.CreateManyIfNotExists(
                    request.UserPublicationRoles,
                    sanitisedEmail,
                    _userService.GetUserId(),
                    request.CreatedDate);

                return userInvite;
            })
            .OnSuccess(async userInvite =>
            {
                var userReleaseInvites = await _contentDbContext
                    .UserReleaseInvites
                    .Include(uri => uri.ReleaseVersion)
                    .ThenInclude(rv => rv.Release)
                    .ThenInclude(r => r.Publication)
                    .Where(uri => uri.Email.ToLower() == sanitisedEmail)
                    .ToListAsync();

                var userPublicationInvites = await _contentDbContext
                    .UserPublicationInvites
                    .Include(upi => upi.Publication)
                    .Where(upi => upi.Email.ToLower() == sanitisedEmail)
                    .ToListAsync();

                return _emailTemplateService
                    .SendInviteEmail(sanitisedEmail, userReleaseInvites, userPublicationInvites)
                    .OnSuccess(() => userInvite);
            });
    }

    public async Task<Either<ActionResult, Unit>> CancelInvite(string email)
    {
        return await _userService
            .CheckCanManageAllUsers()
            .OnSuccess<ActionResult, Unit, Unit>(async () =>
            {
                var invite = await _usersAndRolesDbContext.UserInvites
                    .AsQueryable()
                    .FirstOrDefaultAsync(i => i.Email.ToLower() == email.ToLower());

                if (invite == null)
                {
                    return ValidationActionResult(InviteNotFound);
                }

                if (invite.Accepted)
                {
                    return ValidationActionResult(InviteAlreadyAccepted);
                }

                _usersAndRolesDbContext.UserInvites.Remove(invite);
                await _usersAndRolesDbContext.SaveChangesAsync();

                return Unit.Instance;
            })
            .OnSuccess(async () =>
            {
                var releaseInvites = await _contentDbContext.UserReleaseInvites
                    .AsQueryable()
                    .Where(i => i.Email.ToLower() == email.ToLower())
                    .ToListAsync();
                _contentDbContext.UserReleaseInvites.RemoveRange(releaseInvites);

                var publicationInvites = await _contentDbContext.UserPublicationInvites
                    .AsQueryable()
                    .Where(i => i.Email.ToLower() == email.ToLower())
                    .ToListAsync();
                _contentDbContext.UserPublicationInvites.RemoveRange(publicationInvites);

                await _contentDbContext.SaveChangesAsync();

                return Unit.Instance;
            });
    }

    public async Task<Either<ActionResult, Unit>> UpdateUser(string userId, string roleId)
    {
        return await _userService
            .CheckCanManageAllUsers()
            .OnSuccess(() => _userRoleService.SetGlobalRole(userId, roleId));
    }

    public async Task<Either<ActionResult, Unit>> DeleteUser(string email)
    {
        return await _userService
            .CheckCanManageAllUsers()
            .OnSuccess(async () =>
            {
                // Delete the Identity Framework user, if found.
                var identityUser = await _usersAndRolesDbContext
                    .Users
                    .SingleOrDefaultAsync(user => user.Email.ToLower() == email.ToLower());

                if (identityUser != null)
                {
                    await _userManager.DeleteAsync(identityUser);
                }

                var internalUser = await _contentDbContext
                    .Users
                    .SingleOrDefaultAsync(user => user.Email.ToLower() == email.ToLower());

                if (identityUser == null && internalUser == null)
                {
                    return new Either<ActionResult, Unit>(new NotFoundResult());
                }

                var globalInvites = await _usersAndRolesDbContext
                    .UserInvites
                    .Where(invite => invite.Email.ToLower() == email.ToLower())
                    .ToListAsync();

                var releaseInvites = await _contentDbContext
                    .UserReleaseInvites
                    .Where(invite => invite.Email.ToLower() == email.ToLower())
                    .ToListAsync();

                var publicationInvites = await _contentDbContext
                    .UserPublicationInvites
                    .Where(invite => invite.Email.ToLower() == email.ToLower())
                    .ToListAsync();

                var userReleaseRoles = await _contentDbContext
                    .UserReleaseRoles
                    .Where(urr => internalUser != null && urr.UserId == internalUser.Id)
                    .ToListAsync();

                var userPublicationRoles = await _contentDbContext
                    .UserPublicationRoles
                    .Where(upr => internalUser != null && upr.UserId == internalUser.Id)
                    .ToListAsync();

                if (internalUser is { SoftDeleted: null })
                {
                    internalUser.SoftDeleted = DateTime.UtcNow;
                    internalUser.DeletedById = _userService.GetUserId();
                }

                // Delete any invites that they may have had.
                _usersAndRolesDbContext.UserInvites.RemoveRange(globalInvites);
                _contentDbContext.UserReleaseInvites.RemoveRange(releaseInvites);
                _contentDbContext.UserPublicationInvites.RemoveRange(publicationInvites);
                _contentDbContext.UserReleaseRoles.RemoveRange(userReleaseRoles);
                _contentDbContext.UserPublicationRoles.RemoveRange(userPublicationRoles);

                await _contentDbContext.SaveChangesAsync();
                await _usersAndRolesDbContext.SaveChangesAsync();

                return Unit.Instance;
            });
    }

    private async Task<Either<ActionResult, Unit>> ValidateUserDoesNotExist(string email)
    {
        if (await _usersAndRolesDbContext.Users
                .AsQueryable()
                .AnyAsync(u => u.Email.ToLower() == email.ToLower()))
        {
            return ValidationActionResult(UserAlreadyExists);
        }

        return Unit.Instance;
    }
}

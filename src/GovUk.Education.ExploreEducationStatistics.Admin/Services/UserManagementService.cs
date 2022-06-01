#nullable enable
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using GovUk.Education.ExploreEducationStatistics.Admin.Areas.Identity.Data;
using GovUk.Education.ExploreEducationStatistics.Admin.Areas.Identity.Data.Models;
using GovUk.Education.ExploreEducationStatistics.Admin.Models;
using GovUk.Education.ExploreEducationStatistics.Admin.Services.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Admin.Services.Interfaces.Security;
using GovUk.Education.ExploreEducationStatistics.Admin.ViewModels;
using GovUk.Education.ExploreEducationStatistics.Common.Extensions;
using GovUk.Education.ExploreEducationStatistics.Common.Model;
using GovUk.Education.ExploreEducationStatistics.Common.Services.Interfaces.Security;
using GovUk.Education.ExploreEducationStatistics.Common.Utils;
using GovUk.Education.ExploreEducationStatistics.Content.Model.Database;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using static GovUk.Education.ExploreEducationStatistics.Admin.Models.GlobalRoles;
using static GovUk.Education.ExploreEducationStatistics.Admin.Validators.ValidationUtils;
using static GovUk.Education.ExploreEducationStatistics.Admin.Validators.ValidationErrorMessages;

namespace GovUk.Education.ExploreEducationStatistics.Admin.Services
{
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

        public UserManagementService(UsersAndRolesDbContext usersAndRolesDbContext,
            ContentDbContext contentDbContext,
            IPersistenceHelper<UsersAndRolesDbContext> usersAndRolesPersistenceHelper,
            IEmailTemplateService emailTemplateService,
            IUserRoleService userRoleService,
            IUserService userService,
            IUserInviteRepository userInviteRepository,
            IUserReleaseInviteRepository userReleaseInviteRepository)
        {
            _usersAndRolesDbContext = usersAndRolesDbContext;
            _contentDbContext = contentDbContext;
            _usersAndRolesPersistenceHelper = usersAndRolesPersistenceHelper;
            _emailTemplateService = emailTemplateService;
            _userRoleService = userRoleService;
            _userService = userService;
            _userInviteRepository = userInviteRepository;
            _userReleaseInviteRepository = userReleaseInviteRepository;
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

        public async Task<Either<ActionResult, List<TitleAndIdViewModel>>> ListReleases()
        {
            return await _userService
                .CheckCanManageAllUsers()
                .OnSuccess(_ =>
                {
                    return _contentDbContext.Releases
                        .Include(r => r.Publication)
                        .ToList()
                        .Where(r => r.Publication.IsLatestVersionOfRelease(r.Id))
                        .Select(r => new TitleAndIdViewModel
                        {
                            Id = r.Id,
                            Title = $"{r.Publication.Title} - {r.Title}"
                        })
                        .ToList();
                });
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
                                .OnSuccessCombineWith(_ => _userRoleService.GetPublicationRoles(id))
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

        public async Task<Either<ActionResult, List<UserViewModel>>> ListPendingInvites()
        {
            return await _userService
                .CheckCanManageAllUsers()
                .OnSuccess(async () =>
                    await _usersAndRolesDbContext.UserInvites
                        .AsQueryable()
                        .Where(ui => !ui.Accepted)
                        .OrderBy(ui => ui.Email)
                        .Include(ui => ui.Role)
                        .Select(ui => new UserViewModel
                        {
                            Email = ui.Email,
                            Role = ui.Role.Name,
                        }).ToListAsync()
                );
        }

        public async Task<Either<ActionResult, UserInvite>> InviteUser(
            string email,
            string roleId,
            List<UserReleaseRoleAddViewModel> userReleaseRoles)
        {
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

                    var userInvite = await _userInviteRepository.Create(
                        email: email.ToLower(),
                        roleId: roleId,
                        createdById: _userService.GetUserId());

                    foreach (var userReleaseRole in userReleaseRoles)
                    {
                        await _userReleaseInviteRepository.Create(
                            releaseId: userReleaseRole.ReleaseId,
                            email: email,
                            releaseRole: userReleaseRole.ReleaseRole,
                            emailSent: true, // EES-3403
                            createdById: _userService.GetUserId());
                    }

                    return userInvite;
                })
                .OnSuccess(invite =>
                {
                    return _emailTemplateService
                        .SendInviteEmail(email)
                        .OnSuccess(() => invite);
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
}

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

        public UserManagementService(UsersAndRolesDbContext usersAndRolesDbContext,
            ContentDbContext contentDbContext,
            IPersistenceHelper<UsersAndRolesDbContext> usersAndRolesPersistenceHelper,
            IEmailTemplateService emailTemplateService,
            IUserRoleService userRoleService,
            IUserService userService)
        {
            _usersAndRolesDbContext = usersAndRolesDbContext;
            _contentDbContext = contentDbContext;
            _usersAndRolesPersistenceHelper = usersAndRolesPersistenceHelper;
            _emailTemplateService = emailTemplateService;
            _userRoleService = userRoleService;
            _userService = userService;
        }

        public async Task<Either<ActionResult, List<UserViewModel>>> ListAllUsers()
        {
            return await _userService
                .CheckCanManageAllUsers()
                .OnSuccess(() =>
                {
                    return _usersAndRolesDbContext.Users
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
                        .Where(uvm => uvm.Role != Role.PrereleaseUser.GetEnumLabel())
                        .OrderBy(uvm => uvm.Name)
                        .ToListAsync();
                });
        }

        public async Task<Either<ActionResult, List<TitleAndIdViewModel>>> ListPublications()
        {
            return await _userService
                .CheckCanManageAllUsers()
                .OnSuccess(_ =>
                {
                    return _contentDbContext.Publications
                        .AsQueryable()
                        .Select(p => new TitleAndIdViewModel
                        {
                            Id = p.Id,
                            Title = p.Title
                        })
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
                                .OnSuccessCombineWith(globalRoles => _userRoleService.GetPublicationRoles(id))
                                .OnSuccessCombineWith(globalAndPublicationRoles => _userRoleService.GetReleaseRoles(id))
                                .OnSuccess(tuple =>
                                {
                                    var (globalRoles, publicationRoles, releaseRoles) = tuple;

                                    // Currently we only allow a user to have a maximum of one global role
                                    var globalRole = globalRoles.First();

                                    return new UserViewModel
                                    {
                                        Id = id,
                                        Name = user.FirstName + " " + user.LastName,
                                        Email = user.Email,
                                        Role = globalRole.Id,
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
                            Role = ui.Role.Name
                        }).ToListAsync()
                );
        }

        public async Task<Either<ActionResult, UserInvite>> InviteUser(string email, string roleId)
        {
            return await _userService
                .CheckCanManageAllUsers()
                .OnSuccess<ActionResult, Unit, UserInvite>(async () =>
                {
                    if (string.IsNullOrWhiteSpace(email))
                    {
                        return ValidationActionResult(InvalidEmailAddress);
                    }

                    if (_usersAndRolesDbContext.Users.Any(u => u.Email.ToLower() == email.ToLower()))
                    {
                        return ValidationActionResult(UserAlreadyExists);
                    }

                    var role = await _usersAndRolesDbContext.Roles
                        .AsQueryable()
                        .FirstOrDefaultAsync(r => r.Id == roleId);

                    if (role == null)
                    {
                        return ValidationActionResult(InvalidUserRole);
                    }

                    var invite = new UserInvite
                    {
                        Email = email.ToLower(),
                        Created = DateTime.UtcNow,
                        CreatedById = _userService.GetUserId().ToString(),
                        Role = role
                    };
                    await _usersAndRolesDbContext.UserInvites.AddAsync(invite);
                    await _usersAndRolesDbContext.SaveChangesAsync();
                    return invite;
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
                        .FirstOrDefaultAsync(i => i.Email == email);

                    if (invite == null)
                    {
                        return ValidationActionResult(InviteNotFound);
                    }

                    _usersAndRolesDbContext.UserInvites.Remove(invite);
                    await _usersAndRolesDbContext.SaveChangesAsync();

                    return Unit.Instance;
                });
        }

        public async Task<Either<ActionResult, Unit>> UpdateUser(string userId, string roleId)
        {
            return await _userService
                .CheckCanManageAllUsers()
                .OnSuccess(async () =>
                {
                    // Currently we only allow a user to have a maximum of one global role
                    var existingRole =
                        await _usersAndRolesDbContext.UserRoles
                            .AsQueryable()
                            .FirstOrDefaultAsync(userRole => userRole.UserId == userId);

                    if (existingRole == null)
                    {
                        return await _userRoleService.AddGlobalRole(userId, roleId);
                    }

                    return await _userRoleService.RemoveGlobalRole(userId, existingRole.RoleId)
                        .OnSuccess(() => _userRoleService.AddGlobalRole(userId, roleId));
                });
        }
    }
}

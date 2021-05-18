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
using GovUk.Education.ExploreEducationStatistics.Content.Model;
using GovUk.Education.ExploreEducationStatistics.Content.Model.Database;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using static GovUk.Education.ExploreEducationStatistics.Admin.Validators.ValidationUtils;
using static GovUk.Education.ExploreEducationStatistics.Admin.Validators.ValidationErrorMessages;

namespace GovUk.Education.ExploreEducationStatistics.Admin.Services
{
    public class UserManagementService : IUserManagementService
    {
        private readonly UsersAndRolesDbContext _usersAndRolesDbContext;
        private readonly IUserService _userService;
        private readonly ContentDbContext _contentDbContext;
        private readonly IEmailService _emailService;
        private readonly IConfiguration _configuration;
        private readonly IUserPublicationRoleRepository _userPublicationRoleRepository;
        private readonly IUserReleaseRoleRepository _userReleaseRoleRepository;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IPersistenceHelper<ContentDbContext> _contentPersistenceHelper;
        private readonly IPersistenceHelper<UsersAndRolesDbContext> _usersAndRolesPersistenceHelper;

        public UserManagementService(UsersAndRolesDbContext usersAndRolesDbContext,
            IUserService userService,
            ContentDbContext contentDbContext,
            IEmailService emailService,
            IConfiguration configuration,
            IUserPublicationRoleRepository userPublicationRoleRepository,
            IUserReleaseRoleRepository userReleaseRoleRepository,
            UserManager<ApplicationUser> userManager,
            IPersistenceHelper<ContentDbContext> contentPersistenceHelper,
            IPersistenceHelper<UsersAndRolesDbContext> usersAndRolesPersistenceHelper)
        {
            _usersAndRolesDbContext = usersAndRolesDbContext;
            _userService = userService;
            _contentDbContext = contentDbContext;
            _emailService = emailService;
            _configuration = configuration;
            _userPublicationRoleRepository = userPublicationRoleRepository;
            _userReleaseRoleRepository = userReleaseRoleRepository;
            _userManager = userManager;
            _contentPersistenceHelper = contentPersistenceHelper;
            _usersAndRolesPersistenceHelper = usersAndRolesPersistenceHelper;
        }

        public Task<Either<ActionResult, Dictionary<string, List<string>>>> GetResourceRoles()
        {
            return _userService
                .CheckCanManageAllUsers()
                .OnSuccess(_ =>
                {
                    return new Dictionary<string, List<string>>
                    {
                        {
                            "Publication",
                            Enum.GetNames(typeof(PublicationRole))
                                .OrderBy(name => name)
                                .ToList()
                        },
                        {
                            "Release",
                            Enum.GetNames(typeof(ReleaseRole))
                                .OrderBy(name => name)
                                .ToList()
                        }
                    };
                });
        }

        public async Task<Either<ActionResult, List<UserViewModel>>> ListAllUsers()
        {
            return await _userService
                .CheckCanManageAllUsers()
                .OnSuccess(() =>
                {
                    return _usersAndRolesDbContext.Users
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
                        .Where(uvm => uvm.Role != "Prerelease User")
                        .OrderBy(uvm => uvm.Name)
                        .ToListAsync();
                });
        }

        public Task<Either<ActionResult, Unit>> AddPublicationRole(Guid userId, Guid publicationId, PublicationRole role)
        {
            // EES-2131 TODO
            throw new NotImplementedException();
        }

        public async Task<Either<ActionResult, Unit>> AddReleaseRole(Guid userId, Guid releaseId, ReleaseRole role)
        {
            return await _userService
                .CheckCanManageAllUsers()
                .OnSuccess(async () =>
                {
                    return await _contentPersistenceHelper
                        .CheckEntityExists<Release>(
                            releaseId,
                            q => q.Include(r => r.Publication)
                        )
                        .OnSuccess(release =>
                        {
                            return ValidateReleaseRoleCanBeAdded(userId, releaseId, role)
                                .OnSuccessVoid(async () =>
                                {
                                    var newReleaseRole = new UserReleaseRole
                                    {
                                        ReleaseId = releaseId,
                                        Role = role,
                                        UserId = userId
                                    };

                                    await _contentDbContext.AddAsync(newReleaseRole);
                                    await _contentDbContext.SaveChangesAsync();

                                    SendNewReleaseRoleEmail(userId, release, role);
                                });
                        });
                });
        }

        public async Task<Either<ActionResult, Unit>> RemoveUserPublicationRole(Guid id)
        {
            return await _userService
                .CheckCanManageAllUsers()
                .OnSuccess(async () =>
                {
                    return await _contentPersistenceHelper
                        .CheckEntityExists<UserPublicationRole>(id)
                        .OnSuccessVoid(async userPublicationRole =>
                        {
                            _contentDbContext.Remove(userPublicationRole);
                            await _contentDbContext.SaveChangesAsync();
                        });
                });
        }

        public async Task<Either<ActionResult, Unit>> RemoveUserReleaseRole(Guid id)
        {
            return await _userService
                .CheckCanManageAllUsers()
                .OnSuccess(async () =>
                {
                    return await _contentPersistenceHelper
                        .CheckEntityExists<UserReleaseRole>(id)
                        .OnSuccessVoid(async userReleaseRole =>
                        {
                            _contentDbContext.Remove(userReleaseRole);
                            await _contentDbContext.SaveChangesAsync();
                        });
                });
        }

        public async Task<Either<ActionResult, List<TitleAndIdViewModel>>> ListPublications()
        {
            return await _userService
                .CheckCanManageAllUsers()
                .OnSuccess(_ =>
                {
                    return _contentDbContext.Publications
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
                    return await _usersAndRolesDbContext.Roles.Select(r => new RoleViewModel
                        {
                            Id = r.Id,
                            Name = r.Name,
                            NormalizedName = r.NormalizedName
                        })
                        .OrderBy(x => x.Name)
                        .ToListAsync();
                });
        }

        // TODO EES-2131 Remove me
        public Task<Either<ActionResult, List<EnumExtensions.EnumValue>>> ListReleaseRoles()
        {
            return _userService
                .CheckCanManageAllUsers()
                .OnSuccess(_ => EnumExtensions.GetValues<ReleaseRole>());
        }

        public async Task<List<UserViewModel>> ListPreReleaseUsersAsync()
        {
            return await _usersAndRolesDbContext.Users
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
                .Where(u => u.Role == "Prerelease User")
                .ToListAsync();
        }

        public async Task<Either<ActionResult, UserViewModel>> GetUser(Guid id)
        {
            return await _userService
                .CheckCanManageAllUsers()
                .OnSuccess(async () =>
                {
                    return await _usersAndRolesPersistenceHelper
                        .CheckEntityExists<ApplicationUser>(q => q.Where(user => user.Id == id.ToString()))
                        .OnSuccess(async user =>
                        {
                            var allReleaseRoles = await _contentDbContext.UserReleaseRoles
                                .Include(userReleaseRole => userReleaseRole.Release)
                                .ThenInclude(release => release.Publication)
                                .Where(userReleaseRole => userReleaseRole.UserId == id)
                                .ToListAsync();

                            var latestReleaseRoles = allReleaseRoles
                                .Where(userReleaseRole => userReleaseRole.Release.Publication.IsLatestVersionOfRelease(
                                                              userReleaseRole.Release.Id))
                                .Select(userReleaseRole => new UserReleaseRoleViewModel
                                {
                                    Id = userReleaseRole.Id,
                                    Publication = userReleaseRole.Release.Publication.Title,
                                    Release = userReleaseRole.Release.Title,
                                    Role = userReleaseRole.Role
                                })
                                .OrderBy(x => x.Publication)
                                .ThenBy(x => x.Release)
                                .ToList();

                            return new UserViewModel
                            {
                                Id = id,
                                Name = user.FirstName + " " + user.LastName,
                                Email = user.Email,
                                Role = GetUserRoleId(user.Id),
                                UserReleaseRoles = latestReleaseRoles
                            };
                        });
                });
        }

        public async Task<Either<ActionResult, List<UserViewModel>>> ListPendingInvites()
        {
            return await _userService
                .CheckCanManageAllUsers()
                .OnSuccess(async () =>
                    await _usersAndRolesDbContext.UserInvites
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

        public async Task<Either<ActionResult, UserInvite>> InviteUser(string email, string inviteCreatedByUser,
            string roleId)
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

                    var role = await _usersAndRolesDbContext.Roles.FirstOrDefaultAsync(r => r.Id == roleId);
                    if (role == null)
                    {
                        return ValidationActionResult(InvalidUserRole);
                    }

                    var invite = new UserInvite
                    {
                        Email = email.ToLower(),
                        Created = DateTime.UtcNow,
                        CreatedBy = inviteCreatedByUser,
                        Role = role
                    };
                    await _usersAndRolesDbContext.UserInvites.AddAsync(invite);
                    await _usersAndRolesDbContext.SaveChangesAsync();
                    SendInviteEmail(email);
                    return invite;
                });
        }

        public async Task<Either<ActionResult, Unit>> CancelInvite(string email)
        {
            return await _userService
                .CheckCanManageAllUsers()
                .OnSuccess<ActionResult, Unit, Unit>(async () =>
                {
                    var invite = await _usersAndRolesDbContext.UserInvites.FirstOrDefaultAsync(i => i.Email == email);
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
                .OnSuccess<ActionResult, Unit, Unit>(async () =>
                {
                    var user = await _usersAndRolesDbContext.Users.FirstOrDefaultAsync(i => i.Id == userId);
                    if (user == null)
                    {
                        return ValidationActionResult(UserDoesNotExist);
                    }

                    var userRole = await _usersAndRolesDbContext.UserRoles.FirstOrDefaultAsync(i => i.UserId == userId);
                    if (userRole == null)
                    {
                        return ValidationActionResult(InvalidUserRole);
                    }

                    await _userManager.RemoveFromRoleAsync(user, GetRoleName(userRole.RoleId));
                    await _userManager.AddToRoleAsync(user, GetRoleName(roleId));

                    return Unit.Instance;
                });
        }

        private string GetRoleName(string roleId)
        {
            var userRole = _usersAndRolesDbContext.Roles.FirstOrDefault(r => r.Id == roleId);

            return userRole?.Name;
        }

        private string GetUserRoleId(string userId)
        {
            var userRole = _usersAndRolesDbContext.UserRoles.FirstOrDefault(r => r.UserId == userId);

            return userRole?.RoleId;
        }

        private void SendInviteEmail(string email)
        {
            var uri = _configuration.GetValue<string>("AdminUri");
            var template = _configuration.GetValue<string>("NotifyInviteTemplateId");


            var emailValues = new Dictionary<string, dynamic> {{"url", "https://" + uri}};

            _emailService.SendEmail(email, template, emailValues);
        }

        private void SendNewReleaseRoleEmail(Guid userId, Release release,
            ReleaseRole role)
        {
            var uri = _configuration.GetValue<string>("AdminUri");
            var template = _configuration.GetValue<string>("NotifyReleaseRoleTemplateId");
            var email = _usersAndRolesDbContext.Users
                .First(x => x.Id == userId.ToString())
                .Email;

            var link = (role == ReleaseRole.PrereleaseViewer ? "prerelease " : "summary");
            var emailValues = new Dictionary<string, dynamic>
            {
                {"url", $"https://{uri}/publication/{release.Publication.Id}/release/{release.Id}/{link}"},
                {"role", role.ToString()},
                {"publication", release.Publication.Title},
                {"release", release.Title}
            };

            _emailService.SendEmail(email, template, emailValues);
        }

        private async Task<Either<ActionResult, Unit>> ValidatePublicationRoleCanBeAdded(Guid userId,
            Guid publicationId,
            PublicationRole role)
        {
            if (await _userPublicationRoleRepository.GetByRole(userId, publicationId, role) != null)
            {
                return ValidationActionResult(UserAlreadyHasResourceRole);
            }

            return Unit.Instance;
        }

        private async Task<Either<ActionResult, Unit>> ValidateReleaseRoleCanBeAdded(Guid userId,
            Guid releaseId,
            ReleaseRole role)
        {
            if (await _userReleaseRoleRepository.GetByRole(userId, releaseId, role) != null)
            {
                return ValidationActionResult(UserAlreadyHasResourceRole);
            }

            return Unit.Instance;
        }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using GovUk.Education.ExploreEducationStatistics.Admin.Areas.Identity.Data;
using GovUk.Education.ExploreEducationStatistics.Admin.Areas.Identity.Data.Models;
using GovUk.Education.ExploreEducationStatistics.Admin.Models;
using GovUk.Education.ExploreEducationStatistics.Admin.Services.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Admin.ViewModels;
using GovUk.Education.ExploreEducationStatistics.Common.Extensions;
using GovUk.Education.ExploreEducationStatistics.Common.Model;
using GovUk.Education.ExploreEducationStatistics.Common.Utils;
using GovUk.Education.ExploreEducationStatistics.Content.Model;
using GovUk.Education.ExploreEducationStatistics.Content.Model.Database;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using static GovUk.Education.ExploreEducationStatistics.Admin.Validators.ValidationErrorMessages;
using static GovUk.Education.ExploreEducationStatistics.Admin.Validators.ValidationUtils;

namespace GovUk.Education.ExploreEducationStatistics.Admin.Services
{
    public class UserManagementService : IUserManagementService
    {
        private readonly UsersAndRolesDbContext _usersAndRolesDbContext;
        private readonly ContentDbContext _contentDbContext;
        private readonly IEmailService _emailService;
        private readonly IConfiguration _configuration;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IPersistenceHelper<ContentDbContext> _persistenceHelper;

        public UserManagementService(UsersAndRolesDbContext usersAndRolesDbContext, ContentDbContext contentDbContext,
            IEmailService emailService,
            IConfiguration configuration, UserManager<ApplicationUser> userManager,
            IPersistenceHelper<ContentDbContext> persistenceHelper)
        {
            _usersAndRolesDbContext = usersAndRolesDbContext;
            _contentDbContext = contentDbContext;
            _emailService = emailService;
            _configuration = configuration;
            _userManager = userManager;
            _persistenceHelper = persistenceHelper;
        }

        public async Task<List<UserViewModel>> ListAsync()
        {
            var users = await _usersAndRolesDbContext.Users.Select(u => new UserViewModel
                {
                    Id = u.Id,
                    Name = u.FirstName + " " + u.LastName,
                    Email = u.Email
                }).OrderBy(x => x.Name)
                .ToListAsync();

            foreach (var user in users)
            {
                user.Role = GetUserRoleName(user.Id);
            }

            return users.Where(u => u.Role != "Prerelease User").ToList();
        }

        public async Task<UserReleaseRoleViewModel> GetUserReleaseRole(Guid userId, Guid userReleaseRoleId)
        {
            return await _contentDbContext.UserReleaseRoles
                .Where(x => x.Id == userReleaseRoleId && x.UserId == userId)
                .Select(x => new UserReleaseRoleViewModel
                {
                    Id = x.Id,
                    Publication = _contentDbContext.Publications
                        .Where(p => p.Releases.Any(r => r.Id == x.ReleaseId))
                        .Select(p => new IdTitlePair {Id = p.Id, Title = p.Title}).FirstOrDefault(),
                    Release = _contentDbContext.Releases
                        .Where(r => r.Id == x.ReleaseId)
                        .Select(r => new IdTitlePair {Id = r.Id, Title = r.Title}).FirstOrDefault(),
                    ReleaseRole = new EnumExtensions.EnumValue {Name = x.Role.GetEnumLabel(), Value = 0}
                }).FirstOrDefaultAsync();
        }

        public async Task<Either<ActionResult, UserReleaseRoleViewModel>> AddUserReleaseRole(Guid userId,
            UserReleaseRoleRequest userReleaseRole)
        {
            return await _persistenceHelper
                .CheckEntityExists<Release>(userReleaseRole.ReleaseId)
                .OnSuccess(_ => ValidateUserReleaseRoleCanBeAdded(userId, userReleaseRole))
                .OnSuccess(async _ =>
                {
                    var newReleaseRole = new UserReleaseRole
                    {
                        ReleaseId = userReleaseRole.ReleaseId,
                        Role = userReleaseRole.ReleaseRole,
                        UserId = userId
                    };

                    _contentDbContext.Add(newReleaseRole);

                    await _contentDbContext.SaveChangesAsync();

                    var response = await GetUserReleaseRole(userId, newReleaseRole.Id);

                    SendNewReleaseRoleEmail(userId, response.Publication, response.Release, response.ReleaseRole);

                    return response;
                });
        }


        public async Task<Either<ActionResult, bool>> RemoveUserReleaseRole(Guid userId, Guid userReleaseRoleId)
        {
            return await _persistenceHelper
                .CheckEntityExists<UserReleaseRole>(userReleaseRoleId)
                .OnSuccess(async () =>
                {
                    var entityToRemove = await _contentDbContext.UserReleaseRoles.FirstOrDefaultAsync(r =>
                            r.Id == userReleaseRoleId && r.UserId == userId);
                    _contentDbContext.Remove(entityToRemove);
                    await _contentDbContext.SaveChangesAsync();
                    return true;
                });
        }

        public async Task<List<IdTitlePair>> ListReleasesAsync()
        {
            var releases = await _contentDbContext.Releases
                .Include(r => r.Publication)
                .ToListAsync();
                
            return releases.Where(r => IsLatestVersionOfRelease(r.Publication.Releases, r.Id))
                .Select(r => new IdTitlePair
            {
                Id = r.Id,
                Title = $"{r.Publication.Title} - {r.Title}",
            }).ToList();
        }

        public async Task<List<RoleViewModel>> ListRolesAsync()
        {
            var roles = await _usersAndRolesDbContext.Roles.Select(r => new RoleViewModel()
                {
                    Id = r.Id,
                    Name = r.Name,
                    NormalizedName = r.NormalizedName
                }).OrderBy(x => x.Name)
                .ToListAsync();

            return roles.Where(r => r.NormalizedName != "PRERELEASE USER").ToList();
        }

        public async Task<List<UserViewModel>> ListPreReleaseUsersAsync()
        {
            var users = await _usersAndRolesDbContext.Users.Select(u => new UserViewModel
                {
                    Id = u.Id,
                    Name = u.FirstName + " " + u.LastName,
                    Email = u.Email
                }).OrderBy(x => x.Name)
                .ToListAsync();

            // Potentially user role could be null in the above result in an empty array so assign role afterwards
            foreach (var user in users)
            {
                user.Role = GetUserRoleName(user.Id);
            }

            return users.Where(u => u.Role == "Prerelease User").ToList();
        }

        public async Task<UserViewModel> GetAsync(string userId)
        {
            var user = await _usersAndRolesDbContext.Users.FirstOrDefaultAsync(u => u.Id == userId);

            if (user != null)
            {
                return new UserViewModel
                {
                    Id = user.Id,
                    Name = user.FirstName + " " + user.LastName,
                    Email = user.Email,
                    Role = GetUserRoleId(user.Id),
                    UserReleaseRoles = GetUserReleaseRoles(user.Id)
                };
            }

            return null;
        }

        public async Task<List<UserViewModel>> ListPendingAsync()
        {
            var pendingUsers = await _usersAndRolesDbContext.UserInvites.Where(u => u.Accepted == false)
                .OrderBy(x => x.Email).Select(u => new UserViewModel
                {
                    Email = u.Email,
                    Role = u.Role.Name
                }).ToListAsync();

            return pendingUsers;
        }

        // TODO: Part 2: Switch this to and Either result with validation errors
        // TODO: Part 2: Verify the role exists
        // TODO: Part 2: Verify valid email address
        public async Task<bool> InviteAsync(string email, string user, string roleId)
        {
            if (_usersAndRolesDbContext.Users.Any(u => u.Email == email) || string.IsNullOrWhiteSpace(email))
                return false;

            try
            {
                if (_usersAndRolesDbContext.UserInvites.Any(i => i.Email == email) == false)
                {
                    // TODO add role selection to Invite Users UI
                    var analystRole = await _usersAndRolesDbContext
                        .Roles
                        .Where(r => r.Id == roleId)
                        .FirstAsync();

                    var invite = new UserInvite
                    {
                        Email = email,
                        Created = DateTime.UtcNow,
                        CreatedBy = user,
                        Role = analystRole
                    };

                    await _usersAndRolesDbContext.UserInvites.AddAsync(invite);
                    await _usersAndRolesDbContext.SaveChangesAsync();
                }

                if (_usersAndRolesDbContext.UserInvites.Any(i => i.Email == email && i.Accepted == false))
                {
                    SendInviteEmail(email);
                }

                return true;
            }
            catch
            {
                return false;
            }
        }

        public async Task<bool> CancelInviteAsync(string email)
        {
            var invite = _usersAndRolesDbContext.UserInvites.FirstOrDefault(i => i.Email == email);
            _usersAndRolesDbContext.UserInvites.Remove(invite);

            await _usersAndRolesDbContext.SaveChangesAsync();

            return true;
        }

        public async Task<bool> UpdateAsync(string userId, string roleId)
        {
            var user = await _usersAndRolesDbContext.Users.FirstOrDefaultAsync(i => i.Id == userId);
            var userRole = await _usersAndRolesDbContext.UserRoles.FirstOrDefaultAsync(i => i.UserId == userId);

            if (user == null || userRole == null)
            {
                return false;
            }

            await _userManager.RemoveFromRoleAsync(user, GetRoleName(userRole.RoleId));
            await _userManager.AddToRoleAsync(user, GetRoleName(roleId));

            return true;
        }

        public List<UserReleaseRoleViewModel> GetUserReleaseRoles(string userId)
        {
            return _contentDbContext.UserReleaseRoles
                .Include(urr => urr.Release)
                .ThenInclude(r => r.Publication)
                .Where(x => x.UserId == Guid.Parse(userId))
                .ToList()
                .Where(urr => IsLatestVersionOfRelease(urr.Release.Publication.Releases, urr.Release.Id))
                .Select(x => new UserReleaseRoleViewModel
                {
                    Id = x.Id,
                    Publication = _contentDbContext.Publications
                        .Where(p => p.Releases.Any(r => r.Id == x.ReleaseId))
                        .Select(p => new IdTitlePair {Id = p.Id, Title = p.Title}).FirstOrDefault(),
                    Release = _contentDbContext.Releases
                        .Where(r => r.Id == x.ReleaseId)
                        .Select(r => new IdTitlePair {Id = r.Id, Title = r.Title}).FirstOrDefault(),
                    ReleaseRole = new EnumExtensions.EnumValue {Name = x.Role.GetEnumLabel(), Value = 0}
                })
                .ToList()
                .OrderBy(x => x.Publication.Title)
                .ThenBy(x => x.Release.Title)
                .ToList();
        }

        private string GetRoleName(string roleId)
        {
            var userRole = _usersAndRolesDbContext.Roles.FirstOrDefault(r => r.Id == roleId);

            return userRole?.Name;
        }

        private string GetUserRoleName(string userId)
        {
            var userRole = _usersAndRolesDbContext.UserRoles.FirstOrDefault(r => r.UserId == userId);

            return userRole == null
                ? null
                : _usersAndRolesDbContext.Roles.FirstOrDefault(r => r.Id == userRole.RoleId)?.Name;
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

        private void SendNewReleaseRoleEmail(Guid userId, IdTitlePair publication, IdTitlePair release, EnumExtensions.EnumValue role)
        {
            var uri = _configuration.GetValue<string>("AdminUri");
            var template = _configuration.GetValue<string>("NotifyReleaseRoleTemplateId");
            var email = _usersAndRolesDbContext.Users.FirstOrDefault(x => x.Id == userId.ToString())?.Email;

            var link = (role.Name == ReleaseRole.PrereleaseViewer.GetEnumLabel() ? "prerelease " : "summary");
            var emailValues = new Dictionary<string, dynamic>
            {
                {"url", $"https://{uri}/publication/{publication.Id}/release/{release.Id}/{link}"},
                {"role", role.Name},
                {"publication", publication.Title},
                {"release", release.Title}
            };

            _emailService.SendEmail(email, template, emailValues);
        }

        private async Task<Either<ActionResult, bool>> ValidateUserReleaseRoleCanBeAdded(Guid userId,
            UserReleaseRoleRequest userReleaseRole)
        {
            var existing = await _contentDbContext.UserReleaseRoles.FirstOrDefaultAsync(r =>
                r.UserId == userId && r.ReleaseId == userReleaseRole.ReleaseId &&
                r.Role == userReleaseRole.ReleaseRole);

            if (existing != null)
            {
                return ValidationActionResult(UserAlreadyHasReleaseRole);
            }
            
            return true;
        }
        
        private static bool IsLatestVersionOfRelease(IEnumerable<Release> releases, Guid releaseId)
        {
            return !releases.Any(r => r.PreviousVersionId == releaseId && r.Id != releaseId);
        }
    }
}
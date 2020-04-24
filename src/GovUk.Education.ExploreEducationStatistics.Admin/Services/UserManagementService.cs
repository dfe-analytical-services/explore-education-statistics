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
using GovUk.Education.ExploreEducationStatistics.Content.Model.Database;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;

namespace GovUk.Education.ExploreEducationStatistics.Admin.Services
{
    public class UserManagementService : IUserManagementService
    {
        private readonly UsersAndRolesDbContext _usersAndRolesDbContext;
        private readonly ContentDbContext _contentDbContext;
        private readonly IEmailService _emailService;
        private readonly IConfiguration _configuration;
        private readonly UserManager<ApplicationUser> _userManager;


        public UserManagementService(UsersAndRolesDbContext usersAndRolesDbContext, ContentDbContext contentDbContext,
            IEmailService emailService,
            IConfiguration configuration, UserManager<ApplicationUser> userManager)
        {
            _usersAndRolesDbContext = usersAndRolesDbContext;
            _contentDbContext = contentDbContext;
            _emailService = emailService;
            _configuration = configuration;
            _userManager = userManager;
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

        private List<UserReleaseRole> GetUserReleaseRoles(string userId)
        {
            return _contentDbContext.UserReleaseRoles
                .Where(x => x.UserId == Guid.Parse(userId))
                .Select(x => new UserReleaseRole
                {
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
    }
}
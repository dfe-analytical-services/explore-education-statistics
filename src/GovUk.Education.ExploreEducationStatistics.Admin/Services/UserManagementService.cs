using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using GovUk.Education.ExploreEducationStatistics.Admin.Areas.Identity.Data;
using GovUk.Education.ExploreEducationStatistics.Admin.Areas.Identity.Data.Models;
using GovUk.Education.ExploreEducationStatistics.Admin.Services.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Admin.ViewModels;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;

namespace GovUk.Education.ExploreEducationStatistics.Admin.Services
{
    public class UserManagementService : IUserManagementService
    {
        private readonly UsersAndRolesDbContext _context;
        private readonly IEmailService _emailService;
        private readonly IConfiguration _configuration;

        public UserManagementService(UsersAndRolesDbContext context, IEmailService emailService,
            IConfiguration configuration)
        {
            _context = context;
            _emailService = emailService;
            _configuration = configuration;
        }

        public async Task<List<UserViewModel>> ListAsync()
        {
            var users = await _context.Users.Select(u => new UserViewModel
                {
                    Id = u.Id,
                    Name = u.FirstName + " " + u.LastName,
                    Email = u.Email
                }).OrderBy(x => x.Name)
                .ToListAsync();

            foreach (var user in users)
            {
                user.Role = GetUserRole(user.Id);
            }

            return users.Where(u => u.Role != "Prerelease User").ToList();
        }

        public async Task<List<RoleViewModel>> ListRolesAsync()
        {
            var roles = await _context.Roles.Select(r => new RoleViewModel()
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
            var users = await _context.Users.Select(u => new UserViewModel
                {
                    Id = u.Id,
                    Name = u.FirstName + " " + u.LastName,
                    Email = u.Email
                }).OrderBy(x => x.Name)
                .ToListAsync();

            foreach (var user in users)
            {
                user.Role = GetUserRole(user.Id);
            }

            return users.Where(u => u.Role == "Prerelease User").ToList();
        }

        public async Task<UserViewModel> GetAsync(string userId)
        {
            var user = await _context.Users.FirstOrDefaultAsync(u => u.Id == userId);

            if (user != null)
            {
                return new UserViewModel
                {
                    Id = user.Id,
                    Name = user.FirstName + " " + user.LastName,
                    Email = user.Email,
                    Role = GetUserRole(user.Id)
                };
            }

            return null;
        }

        public async Task<List<UserViewModel>> ListPendingAsync()
        {
            // Bit of a hack to get the role of the user.
            var pendingUsers = await _context.UserInvites.Where(u => u.Accepted == false)
                .OrderBy(x => x.Email).Select(u => new UserViewModel
                {
                    Email = u.Email,
                    Role = u.Role.Name
                }).ToListAsync();

            return pendingUsers;
        }

        public async Task<bool> InviteAsync(string email, string user, string roleId)
        {
            if (_context.Users.Any(u => u.Email == email) || string.IsNullOrWhiteSpace(email)) return false;

            try
            {
                if (_context.UserInvites.Any(i => i.Email == email) == false)
                {
                    // TODO add role selection to Invite Users UI
                    var analystRole = await _context
                        .Roles
                        // TODO represent roles with an enum
                        .Where(r => r.Id == roleId)
                        .FirstAsync();

                    var invite = new UserInvite
                    {
                        Email = email,
                        Created = DateTime.UtcNow,
                        CreatedBy = user,
                        Role = analystRole
                    };

                    await _context.UserInvites.AddAsync(invite);
                    await _context.SaveChangesAsync();
                }

                if (_context.UserInvites.Any(i => i.Email == email && i.Accepted == false))
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

        private string GetUserRole(string userId)
        {
            var userRole = _context.UserRoles.FirstOrDefault(r => r.UserId == userId);

            return _context.Roles.FirstOrDefault(r => r.Id == userRole.RoleId)?.Name;
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
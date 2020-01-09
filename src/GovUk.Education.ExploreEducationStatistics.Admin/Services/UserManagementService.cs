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

            return users;
        }

        public async Task<List<string>> ListPendingAsync()
        {
            var pendingUsers = await _context.UserInvites.Where(u => u.Accepted == false).Select(u => u.Email)
                .OrderBy(x => x).ToListAsync();

            return pendingUsers;
        }

        public async Task<bool> InviteAsync(string email, string user)
        {
            if (_context.Users.Any(u => u.Email == email) || string.IsNullOrWhiteSpace(email)) return false;

            try
            {
                if (_context.UserInvites.Any(i => i.Email == email) == false)
                {
                    var invite = new UserInvite
                    {
                        Email = email,
                        Created = DateTime.UtcNow,
                        CreatedBy = user
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

        private void SendInviteEmail(string email)
        {
            var uri = _configuration.GetValue<string>("AdminUri");
            var template = _configuration.GetValue<string>("NotifyInviteTemplateId");


            var emailValues = new Dictionary<string, dynamic> {{"url", "https://" + uri}};

            _emailService.SendEmail(email, template, emailValues);
        }
    }
}
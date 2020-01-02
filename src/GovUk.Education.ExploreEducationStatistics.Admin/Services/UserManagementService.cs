using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using GovUk.Education.ExploreEducationStatistics.Admin.Areas.Identity.Data;
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
        private const string TemplateId = "a183f2ae-a859-49ad-9c26-f648f60d9f61";

        public UserManagementService(UsersAndRolesDbContext context, IEmailService emailService, IConfiguration configuration)
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

        public async Task<bool> InviteAsync(string email)
        {
            if (_context.Users.Any(u => u.Email == email) || string.IsNullOrWhiteSpace(email)) return false;

            try
            {
                // TODO: add user to user invite table
                
                SendInviteEmail(email);

                return true;
            }
            catch
            {
                return false;
            }
        }

        private void SendInviteEmail(string email)
        {
            var emailValues = new Dictionary<string, dynamic> {{"url", "https://" + _configuration.GetValue<string>("AdminUri")}};

            _emailService.SendEmail(email, TemplateId, emailValues);
        }
    }
}
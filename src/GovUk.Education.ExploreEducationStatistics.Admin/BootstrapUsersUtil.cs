using System;
using System.Linq;
using GovUk.Education.ExploreEducationStatistics.Admin.Areas.Identity.Data;
using GovUk.Education.ExploreEducationStatistics.Admin.Areas.Identity.Data.Models;
using GovUk.Education.ExploreEducationStatistics.Content.Model.Database;
using IdentityServer4.Extensions;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace GovUk.Education.ExploreEducationStatistics.Admin
{
    public static class BootstrapUsersUtil
    {
        /**
         * Add any bootstrapping BAU users that we have specified on startup. 
         */
        public static void AddBootstrapUsers(
            IApplicationBuilder app,
            IWebHostEnvironment env,
            IConfiguration configuration)
        {
            if (!env.IsDevelopment())
            {
                throw new Exception("Cannot add bootstrap users in non-Development environments");
            }
            
            var bauBootstrapUserEmailAddresses = configuration
                .GetSection("BootstrapUsers")?
                .GetValue<string>("BAU")?
                .Split(',');

            if (bauBootstrapUserEmailAddresses.IsNullOrEmpty())
            {
                return;
            }

            using var serviceScope = app.ApplicationServices.GetRequiredService<IServiceScopeFactory>()
                .CreateScope();
            using var usersAndRolesDb = serviceScope.ServiceProvider.GetService<UsersAndRolesDbContext>();
            using var contentDb = serviceScope.ServiceProvider.GetService<ContentDbContext>();

            var bauRole = usersAndRolesDb.Roles.First(r => r.Name.Equals("BAU User"));

            var existingEmailInvites = usersAndRolesDb
                .UserInvites
                .Select(i => i.Email.ToLower())
                .ToList();

            var existingUserEmails = contentDb
                .Users
                .Select(u => u.Email.ToLower())
                .ToList();

            var newInvitesToCreate = bauBootstrapUserEmailAddresses
                .Where(email =>
                    !existingEmailInvites.Contains(email.ToLower()) &&
                    !existingUserEmails.Contains(email.ToLower()))
                .Select(email =>
                    new UserInvite
                    {
                        Email = email,
                        Role = bauRole,
                        Accepted = false,
                        Created = DateTime.UtcNow,
                    });

            if (newInvitesToCreate.IsNullOrEmpty())
            {
                return;
            }

            usersAndRolesDb.UserInvites.AddRange(newInvitesToCreate);
            usersAndRolesDb.SaveChanges();
        }
    }
}
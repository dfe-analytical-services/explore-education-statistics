#nullable enable
using System;
using System.Linq;
using GovUk.Education.ExploreEducationStatistics.Admin.Database;
using GovUk.Education.ExploreEducationStatistics.Admin.Models;
using GovUk.Education.ExploreEducationStatistics.Common.Extensions;
using GovUk.Education.ExploreEducationStatistics.Content.Model.Database;
using Microsoft.Extensions.Configuration;
using static GovUk.Education.ExploreEducationStatistics.Admin.Models.GlobalRoles;

namespace GovUk.Education.ExploreEducationStatistics.Admin.Services;

public class BootstrapUsersService
{
    private readonly IConfiguration _configuration;
    private readonly UsersAndRolesDbContext _usersAndRolesDbContext;
    private readonly ContentDbContext _contentDbContext;

    public BootstrapUsersService(
        IConfiguration configuration,
        UsersAndRolesDbContext usersAndRolesDbContext,
        ContentDbContext contentDbContext)
    {
        _configuration = configuration;
        _usersAndRolesDbContext = usersAndRolesDbContext;
        _contentDbContext = contentDbContext;
    }

    /**
     * Add any bootstrapping BAU users that we have specified on startup.
     */
    public void AddBootstrapUsers()
    {
        var bauBootstrapUserEmailAddresses = _configuration
            .GetSection("BootstrapUsers")?
            .GetValue<string>("BAU")?
            .Split(',');

        if (bauBootstrapUserEmailAddresses.IsNullOrEmpty())
        {
            return;
        }

        var existingEmailInvites = _usersAndRolesDbContext
            .UserInvites
            .AsQueryable()
            .Select(i => i.Email.ToLower())
            .ToList();

        var existingUserEmails = _contentDbContext
            .Users
            .AsQueryable()
            .Select(u => u.Email.ToLower())
            .ToList();

        var newInvitesToCreate = bauBootstrapUserEmailAddresses!
            .Where(email =>
                !existingEmailInvites.Contains(email.ToLower()) &&
                !existingUserEmails.Contains(email.ToLower()))
            .Select(email =>
                new UserInvite
                {
                    Email = email,
                    RoleId = Role.BauUser.GetEnumValue(),
                    Accepted = false,
                    Created = DateTime.UtcNow,
                })
            .ToList();

        if (newInvitesToCreate.IsNullOrEmpty())
        {
            return;
        }

        _usersAndRolesDbContext.UserInvites.AddRange(newInvitesToCreate);
        _usersAndRolesDbContext.SaveChanges();
    }
}

#nullable enable
using GovUk.Education.ExploreEducationStatistics.Common.Extensions;
using GovUk.Education.ExploreEducationStatistics.Content.Model;
using GovUk.Education.ExploreEducationStatistics.Content.Model.Database;
using static GovUk.Education.ExploreEducationStatistics.Admin.Models.GlobalRoles;

namespace GovUk.Education.ExploreEducationStatistics.Admin.Services;

public class BootstrapUsersService(IConfiguration configuration, ContentDbContext contentDbContext)
{
    /**
     * Add any bootstrapping BAU users that we have specified on startup.
     */
    public void AddBootstrapUsers()
    {
        var bauBootstrapUserEmailAddresses = configuration
            .GetSection("BootstrapUsers")
            ?.GetValue<string>("BAU")
            ?.Split(',', StringSplitOptions.RemoveEmptyEntries)
            .Select(email => email.Trim())
            .Where(email => !email.IsNullOrWhitespace());

        if (bauBootstrapUserEmailAddresses.IsNullOrEmpty())
        {
            return;
        }

        var existingUserEmails = contentDbContext.Users.Select(u => u.Email.ToLower()).ToList();

        var placeholderDeletedUserId = contentDbContext
            .Users.Where(u => u.Email == User.DeletedUserPlaceholderEmail)
            .Select(u => u.Id)
            .Single();

        var newInvitesToCreate = bauBootstrapUserEmailAddresses!
            .Where(email => !existingUserEmails.Contains(email.ToLower()))
            .Select(email => new User
            {
                Email = email,
                RoleId = Role.BauUser.GetEnumValue(),
                Active = false,
                Created = DateTime.UtcNow,
                CreatedById = placeholderDeletedUserId,
            })
            .ToList();

        if (newInvitesToCreate.IsNullOrEmpty())
        {
            return;
        }

        contentDbContext.Users.AddRange(newInvitesToCreate);
        contentDbContext.SaveChanges();
    }
}

#nullable enable
using System.Threading.Tasks;
using GovUk.Education.ExploreEducationStatistics.Admin.Models;
using GovUk.Education.ExploreEducationStatistics.Admin.Options;
using GovUk.Education.ExploreEducationStatistics.Content.Model;
using GovUk.Education.ExploreEducationStatistics.Content.Model.Database;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.Options;

namespace GovUk.Education.ExploreEducationStatistics.Admin.Security.AuthorizationHandlers;

public class DeleteTestReleaseRequirement : IAuthorizationRequirement;

public class DeleteTestReleaseAuthorizationHandler(
    IOptions<AppOptions> appOptions)
    : AuthorizationHandler<DeleteTestReleaseRequirement, ReleaseVersion>
{
    protected override Task HandleRequirementAsync(
        AuthorizationHandlerContext context,
        DeleteTestReleaseRequirement requirement,
        ReleaseVersion releaseVersion)
    {
        if (!appOptions.Value.EnableThemeDeletion)
        {
            return Task.CompletedTask;
        }

        if (!context.User.IsInRole(GlobalRoles.Role.BauUser.ToString()))
        {
            return Task.CompletedTask;
        }

        var topic = releaseVersion.Publication.Topic;

        if (topic.Title.StartsWith("UI test topic") && !topic.Title.StartsWith("Seed topic"))
        {
            context.Succeed(requirement);
        }

        return Task.CompletedTask;
    }
}

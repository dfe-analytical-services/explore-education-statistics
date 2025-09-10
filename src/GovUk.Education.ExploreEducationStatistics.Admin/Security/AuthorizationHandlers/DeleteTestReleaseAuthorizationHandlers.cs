#nullable enable
using GovUk.Education.ExploreEducationStatistics.Admin.Models;
using GovUk.Education.ExploreEducationStatistics.Admin.Options;
using GovUk.Education.ExploreEducationStatistics.Common.Extensions;
using GovUk.Education.ExploreEducationStatistics.Content.Model;
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

        if (!context.User.IsInRole(GlobalRoles.Role.BauUser.GetEnumLabel()))
        {
            return Task.CompletedTask;
        }

        var theme = releaseVersion.Publication.Theme;

        if (theme.IsTestOrSeedTheme())
        {
            context.Succeed(requirement);
        }

        return Task.CompletedTask;
    }
}

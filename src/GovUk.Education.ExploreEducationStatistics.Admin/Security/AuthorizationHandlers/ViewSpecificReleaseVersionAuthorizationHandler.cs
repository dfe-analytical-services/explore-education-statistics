#nullable enable
using GovUk.Education.ExploreEducationStatistics.Content.Model;
using GovUk.Education.ExploreEducationStatistics.Content.Security.AuthorizationHandlers;
using Microsoft.AspNetCore.Authorization;

namespace GovUk.Education.ExploreEducationStatistics.Admin.Security.AuthorizationHandlers;

public class ViewSpecificReleaseVersionAuthorizationHandler(IAuthorizationHandlerService authorizationHandlerService)
    : AuthorizationHandler<ViewReleaseVersionRequirement, ReleaseVersion>
{
    protected override async Task HandleRequirementAsync(
        AuthorizationHandlerContext context,
        ViewReleaseVersionRequirement requirement,
        ReleaseVersion releaseVersion
    )
    {
        if (await authorizationHandlerService.IsReleaseVersionViewableByUser(releaseVersion, context.User))
        {
            context.Succeed(requirement);
        }
    }
}

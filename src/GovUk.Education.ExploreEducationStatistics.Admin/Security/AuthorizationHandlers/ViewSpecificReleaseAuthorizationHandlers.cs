using GovUk.Education.ExploreEducationStatistics.Content.Model;
using GovUk.Education.ExploreEducationStatistics.Content.Security.AuthorizationHandlers;
using Microsoft.AspNetCore.Authorization;

namespace GovUk.Education.ExploreEducationStatistics.Admin.Security.AuthorizationHandlers;

public class ViewSpecificReleaseAuthorizationHandler(IAuthorizationHandlerService authorizationHandlerService)
    : AuthorizationHandler<ViewReleaseRequirement, ReleaseVersion>
{
    protected override async Task HandleRequirementAsync(
        AuthorizationHandlerContext context,
        ViewReleaseRequirement requirement,
        ReleaseVersion releaseVersion
    )
    {
        if (await authorizationHandlerService.IsReleaseVersionViewableByUser(releaseVersion, context.User))
        {
            context.Succeed(requirement);
        }
    }
}

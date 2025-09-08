using GovUk.Education.ExploreEducationStatistics.Content.Model;
using GovUk.Education.ExploreEducationStatistics.Content.Security.AuthorizationHandlers;
using Microsoft.AspNetCore.Authorization;

namespace GovUk.Education.ExploreEducationStatistics.Admin.Security.AuthorizationHandlers;

public class ViewSpecificReleaseAuthorizationHandler : AuthorizationHandler<ViewReleaseRequirement, ReleaseVersion>
{
    private readonly AuthorizationHandlerService _authorizationHandlerService;

    public ViewSpecificReleaseAuthorizationHandler(AuthorizationHandlerService authorizationHandlerService)
    {
        _authorizationHandlerService = authorizationHandlerService;
    }

    protected override async Task HandleRequirementAsync(
        AuthorizationHandlerContext context,
        ViewReleaseRequirement requirement,
        ReleaseVersion releaseVersion
    )
    {
        if (await _authorizationHandlerService.IsReleaseVersionViewableByUser(releaseVersion, context.User))
        {
            context.Succeed(requirement);
        }
    }
}

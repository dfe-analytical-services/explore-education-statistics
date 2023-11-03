using System.Threading.Tasks;
using GovUk.Education.ExploreEducationStatistics.Admin.Services.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Content.Model;
using GovUk.Education.ExploreEducationStatistics.Content.Security.AuthorizationHandlers;
using Microsoft.AspNetCore.Authorization;

namespace GovUk.Education.ExploreEducationStatistics.Admin.Security.AuthorizationHandlers;

public class ViewSpecificReleaseAuthorizationHandler : AuthorizationHandler<ViewReleaseRequirement, Release>
{
    private readonly IPreReleaseService _preReleaseService;
    private readonly AuthorizationHandlerService _authorizationHandlerService;

    public ViewSpecificReleaseAuthorizationHandler(
        IPreReleaseService preReleaseService,
        AuthorizationHandlerService authorizationHandlerService)
    {
        _preReleaseService = preReleaseService;
        _authorizationHandlerService = authorizationHandlerService;
    }

    protected override async Task HandleRequirementAsync(
        AuthorizationHandlerContext context,
        ViewReleaseRequirement requirement,
        Release release)
    {
        if (await _authorizationHandlerService.IsReleaseViewableByUser(release, context.User))
        {
            context.Succeed(requirement);
        }
    }
}

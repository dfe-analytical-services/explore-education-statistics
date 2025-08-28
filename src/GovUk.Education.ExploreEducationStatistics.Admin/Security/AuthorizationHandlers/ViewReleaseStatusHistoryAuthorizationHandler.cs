using GovUk.Education.ExploreEducationStatistics.Common.Services.Security;
using GovUk.Education.ExploreEducationStatistics.Content.Model;
using Microsoft.AspNetCore.Authorization;
using static GovUk.Education.ExploreEducationStatistics.Admin.Security.AuthorizationHandlers.AuthorizationHandlerService;
using static GovUk.Education.ExploreEducationStatistics.Common.Services.CollectionUtils;

namespace GovUk.Education.ExploreEducationStatistics.Admin.Security.AuthorizationHandlers;

public class ViewReleaseStatusHistoryRequirement : IAuthorizationRequirement
{
}

public class ViewReleaseStatusHistoryAuthorizationHandler
    : AuthorizationHandler<ViewReleaseStatusHistoryRequirement, ReleaseVersion>
{
    private readonly AuthorizationHandlerService _authorizationHandlerService;

    public ViewReleaseStatusHistoryAuthorizationHandler(
        AuthorizationHandlerService authorizationHandlerService)
    {
        _authorizationHandlerService = authorizationHandlerService;
    }

    protected override async Task HandleRequirementAsync(
        AuthorizationHandlerContext context,
        ViewReleaseStatusHistoryRequirement requirement,
        ReleaseVersion releaseVersion)
    {
        if (SecurityUtils.HasClaim(context.User, SecurityClaimTypes.AccessAllReleases))
        {
            context.Succeed(requirement);
            return;
        }
        
        if (await _authorizationHandlerService
                .HasRolesOnPublicationOrReleaseVersion(
                    context.User.GetUserId(),
                    releaseVersion.PublicationId,
                    releaseVersion.Id,
                    ListOf(PublicationRole.Owner, PublicationRole.Allower),
                    UnrestrictedReleaseViewerRoles))
        {
            context.Succeed(requirement);
        }
    }
}

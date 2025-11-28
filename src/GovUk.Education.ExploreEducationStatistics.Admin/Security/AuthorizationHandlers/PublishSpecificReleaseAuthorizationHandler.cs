using GovUk.Education.ExploreEducationStatistics.Common.Services.Security;
using GovUk.Education.ExploreEducationStatistics.Content.Model;
using Microsoft.AspNetCore.Authorization;
using static GovUk.Education.ExploreEducationStatistics.Admin.Security.SecurityClaimTypes;
using static GovUk.Education.ExploreEducationStatistics.Common.Services.CollectionUtils;

namespace GovUk.Education.ExploreEducationStatistics.Admin.Security.AuthorizationHandlers;

public class PublishSpecificReleaseRequirement : IAuthorizationRequirement { }

public class PublishSpecificReleaseAuthorizationHandler
    : AuthorizationHandler<PublishSpecificReleaseRequirement, ReleaseVersion>
{
    private readonly AuthorizationHandlerService _authorizationHandlerService;

    public PublishSpecificReleaseAuthorizationHandler(AuthorizationHandlerService authorizationHandlerService)
    {
        _authorizationHandlerService = authorizationHandlerService;
    }

    protected override async Task HandleRequirementAsync(
        AuthorizationHandlerContext context,
        PublishSpecificReleaseRequirement requirement,
        ReleaseVersion releaseVersion
    )
    {
        if (releaseVersion.ApprovalStatus != ReleaseApprovalStatus.Approved)
        {
            return;
        }

        if (SecurityUtils.HasClaim(context.User, PublishAllReleases))
        {
            context.Succeed(requirement);
            return;
        }

        if (
            await _authorizationHandlerService.HasRolesOnPublicationOrReleaseVersion(
                context.User.GetUserId(),
                releaseVersion.PublicationId,
                releaseVersion.Id,
                SetOf(PublicationRole.Owner),
                SetOf(ReleaseRole.Approver)
            )
        )
        {
            context.Succeed(requirement);
        }
    }
}

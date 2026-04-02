#nullable enable
using GovUk.Education.ExploreEducationStatistics.Common.Services.Security;
using GovUk.Education.ExploreEducationStatistics.Content.Model;
using Microsoft.AspNetCore.Authorization;

namespace GovUk.Education.ExploreEducationStatistics.Admin.Security.AuthorizationHandlers;

public class UpdateSpecificReleaseRequirement : IAuthorizationRequirement;

public class UpdateSpecificReleaseAuthorizationHandler(IAuthorizationHandlerService authorizationHandlerService)
    : AuthorizationHandler<UpdateSpecificReleaseRequirement, Release>
{
    protected override async Task HandleRequirementAsync(
        AuthorizationHandlerContext context,
        UpdateSpecificReleaseRequirement requirement,
        Release release
    )
    {
        if (SecurityUtils.HasClaim(context.User, SecurityClaimTypes.UpdateAllReleases))
        {
            context.Succeed(requirement);
            return;
        }

        if (
            await authorizationHandlerService.UserHasAnyPublicationRoleOnPublication(
                userId: context.User.GetUserId(),
                publicationId: release.PublicationId,
                rolesToInclude: [PublicationRole.Drafter, PublicationRole.Approver]
            )
        )
        {
            context.Succeed(requirement);
        }
    }
}

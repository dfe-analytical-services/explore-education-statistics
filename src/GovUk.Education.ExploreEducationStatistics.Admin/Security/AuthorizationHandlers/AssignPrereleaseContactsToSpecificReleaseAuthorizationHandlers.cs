#nullable enable
using GovUk.Education.ExploreEducationStatistics.Common.Services.Security;
using GovUk.Education.ExploreEducationStatistics.Content.Model;
using Microsoft.AspNetCore.Authorization;

namespace GovUk.Education.ExploreEducationStatistics.Admin.Security.AuthorizationHandlers;

public class AssignPrereleaseContactsToSpecificReleaseRequirement : IAuthorizationRequirement { }

public class AssignPrereleaseContactsToSpecificReleaseAuthorizationHandler(
    IAuthorizationHandlerService authorizationHandlerService
) : AuthorizationHandler<AssignPrereleaseContactsToSpecificReleaseRequirement, ReleaseVersion>
{
    protected override async Task HandleRequirementAsync(
        AuthorizationHandlerContext context,
        AssignPrereleaseContactsToSpecificReleaseRequirement requirement,
        ReleaseVersion releaseVersion
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
                publicationId: releaseVersion.Release.PublicationId,
                rolesToInclude: [PublicationRole.Drafter, PublicationRole.Approver]
            )
        )
        {
            context.Succeed(requirement);
        }
    }
}

#nullable enable
using GovUk.Education.ExploreEducationStatistics.Common.Services.Security;
using GovUk.Education.ExploreEducationStatistics.Content.Model;
using Microsoft.AspNetCore.Authorization;

namespace GovUk.Education.ExploreEducationStatistics.Admin.Security.AuthorizationHandlers;

public class ViewSpecificPreReleaseSummaryRequirement : IAuthorizationRequirement { }

public class ViewSpecificPreReleaseSummaryAuthorizationHandler(IAuthorizationHandlerService authorizationHandlerService)
    : AuthorizationHandler<ViewSpecificPreReleaseSummaryRequirement, ReleaseVersion>
{
    protected override async Task HandleRequirementAsync(
        AuthorizationHandlerContext context,
        ViewSpecificPreReleaseSummaryRequirement requirement,
        ReleaseVersion releaseVersion
    )
    {
        if (SecurityUtils.HasClaim(context.User, SecurityClaimTypes.AccessAllReleases))
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
            return;
        }

        if (
            await authorizationHandlerService.UserHasPreReleaseRoleOnReleaseVersion(
                userId: context.User.GetUserId(),
                releaseVersionId: releaseVersion.Id
            )
        )
        {
            context.Succeed(requirement);
        }
    }
}

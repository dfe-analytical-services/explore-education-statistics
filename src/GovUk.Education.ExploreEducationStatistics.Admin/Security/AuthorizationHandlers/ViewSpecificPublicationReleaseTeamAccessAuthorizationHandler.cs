#nullable enable
using GovUk.Education.ExploreEducationStatistics.Common.Services.Security;
using GovUk.Education.ExploreEducationStatistics.Content.Model;
using Microsoft.AspNetCore.Authorization;

namespace GovUk.Education.ExploreEducationStatistics.Admin.Security.AuthorizationHandlers;

public class ViewSpecificPublicationReleaseTeamAccessRequirement : IAuthorizationRequirement { }

public class ViewSpecificPublicationReleaseTeamAccessAuthorizationHandler(
    IAuthorizationHandlerService authorizationHandlerService
) : AuthorizationHandler<ViewSpecificPublicationReleaseTeamAccessRequirement, Publication>
{
    protected override async Task HandleRequirementAsync(
        AuthorizationHandlerContext context,
        ViewSpecificPublicationReleaseTeamAccessRequirement requirement,
        Publication publication
    )
    {
        if (SecurityUtils.HasClaim(context.User, SecurityClaimTypes.AccessAllPublications))
        {
            context.Succeed(requirement);
            return;
        }

        if (
            await authorizationHandlerService.UserHasAnyPublicationRoleOnPublication(
                userId: context.User.GetUserId(),
                publicationId: publication.Id,
                rolesToInclude: [PublicationRole.Drafter, PublicationRole.Approver]
            )
        )
        {
            context.Succeed(requirement);
        }
    }
}

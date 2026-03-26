#nullable enable
using GovUk.Education.ExploreEducationStatistics.Common.Services.Security;
using GovUk.Education.ExploreEducationStatistics.Content.Model;
using Microsoft.AspNetCore.Authorization;

namespace GovUk.Education.ExploreEducationStatistics.Admin.Security.AuthorizationHandlers;

public class DropMethodologyLinkRequirement : IAuthorizationRequirement { }

public class DropMethodologyLinkAuthorizationHandler(IAuthorizationHandlerService authorizationHandlerService)
    : AuthorizationHandler<DropMethodologyLinkRequirement, PublicationMethodology>
{
    protected override async Task HandleRequirementAsync(
        AuthorizationHandlerContext context,
        DropMethodologyLinkRequirement requirement,
        PublicationMethodology link
    )
    {
        if (link.Owner)
        {
            // No user is allowed to drop the link between a methodology and its owning publication
            return;
        }

        // Allow users who can adopt methodologies to also drop them
        if (SecurityUtils.HasClaim(context.User, SecurityClaimTypes.AdoptAnyMethodology))
        {
            context.Succeed(requirement);
            return;
        }

        if (
            await authorizationHandlerService.UserHasAnyPublicationRoleOnPublication(
                userId: context.User.GetUserId(),
                publicationId: link.PublicationId,
                rolesToInclude: [PublicationRole.Drafter, PublicationRole.Approver]
            )
        )
        {
            context.Succeed(requirement);
        }
    }
}

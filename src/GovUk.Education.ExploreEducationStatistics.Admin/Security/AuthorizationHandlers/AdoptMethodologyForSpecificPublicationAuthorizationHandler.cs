#nullable enable
using GovUk.Education.ExploreEducationStatistics.Common.Services.Security;
using GovUk.Education.ExploreEducationStatistics.Content.Model;
using Microsoft.AspNetCore.Authorization;

namespace GovUk.Education.ExploreEducationStatistics.Admin.Security.AuthorizationHandlers;

public class AdoptMethodologyForSpecificPublicationRequirement : IAuthorizationRequirement { }

public class AdoptMethodologyForSpecificPublicationAuthorizationHandler(
    IAuthorizationHandlerService authorizationHandlerService
) : AuthorizationHandler<AdoptMethodologyForSpecificPublicationRequirement, Publication>
{
    protected override async Task HandleRequirementAsync(
        AuthorizationHandlerContext context,
        AdoptMethodologyForSpecificPublicationRequirement requirement,
        Publication publication
    )
    {
        if (SecurityUtils.HasClaim(context.User, SecurityClaimTypes.AdoptAnyMethodology))
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

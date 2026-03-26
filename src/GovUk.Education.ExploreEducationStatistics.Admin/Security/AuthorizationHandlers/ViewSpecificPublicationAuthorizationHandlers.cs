using GovUk.Education.ExploreEducationStatistics.Common.Services.Security;
using GovUk.Education.ExploreEducationStatistics.Content.Model;
using Microsoft.AspNetCore.Authorization;

namespace GovUk.Education.ExploreEducationStatistics.Admin.Security.AuthorizationHandlers;

public class ViewSpecificPublicationRequirement : IAuthorizationRequirement { }

public class ViewSpecificPublicationAuthorizationHandler(IAuthorizationHandlerService authorizationHandlerService)
    : AuthorizationHandler<ViewSpecificPublicationRequirement, Publication>
{
    protected override async Task HandleRequirementAsync(
        AuthorizationHandlerContext context,
        ViewSpecificPublicationRequirement requirement,
        Publication publication
    )
    {
        // If the user has the "AccessAllPublications" Claim, they can see any Publication.
        if (SecurityUtils.HasClaim(context.User, SecurityClaimTypes.AccessAllPublications))
        {
            context.Succeed(requirement);
            return;
        }

        // If the user has any role (including both publication roles and prerelease roles) on the publication, they can see it.
        if (
            await authorizationHandlerService.UserHasAnyRoleOnPublication(
                userId: context.User.GetUserId(),
                publicationId: publication.Id
            )
        )
        {
            context.Succeed(requirement);
        }
    }
}

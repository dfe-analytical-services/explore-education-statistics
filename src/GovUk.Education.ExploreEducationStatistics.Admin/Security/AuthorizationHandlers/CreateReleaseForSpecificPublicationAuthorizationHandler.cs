#nullable enable
using GovUk.Education.ExploreEducationStatistics.Common.Services.Security;
using GovUk.Education.ExploreEducationStatistics.Content.Model;
using Microsoft.AspNetCore.Authorization;

namespace GovUk.Education.ExploreEducationStatistics.Admin.Security.AuthorizationHandlers;

public class CreateReleaseForSpecificPublicationRequirement : IAuthorizationRequirement { }

public class CreateReleaseForSpecificPublicationAuthorizationHandler(
    IAuthorizationHandlerService authorizationHandlerService
) : AuthorizationHandler<CreateReleaseForSpecificPublicationRequirement, Publication>
{
    protected override async Task HandleRequirementAsync(
        AuthorizationHandlerContext context,
        CreateReleaseForSpecificPublicationRequirement requirement,
        Publication publication
    )
    {
        // No user is allowed to create a new release of an archived publication
        if (publication.SupersededById.HasValue)
        {
            return;
        }

        if (SecurityUtils.HasClaim(context.User, SecurityClaimTypes.CreateAnyRelease))
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

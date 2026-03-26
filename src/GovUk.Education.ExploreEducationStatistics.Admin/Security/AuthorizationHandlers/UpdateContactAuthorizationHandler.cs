using GovUk.Education.ExploreEducationStatistics.Common.Services.Security;
using GovUk.Education.ExploreEducationStatistics.Content.Model;
using Microsoft.AspNetCore.Authorization;

namespace GovUk.Education.ExploreEducationStatistics.Admin.Security.AuthorizationHandlers;

public class UpdateContactRequirement : IAuthorizationRequirement { }

public class UpdateContactAuthorizationHandler(IAuthorizationHandlerService authorizationHandlerService)
    : AuthorizationHandler<UpdateContactRequirement, Publication>
{
    protected override async Task HandleRequirementAsync(
        AuthorizationHandlerContext context,
        UpdateContactRequirement contactRequirement,
        Publication publication
    )
    {
        if (SecurityUtils.HasClaim(context.User, SecurityClaimTypes.UpdateAllPublications))
        {
            context.Succeed(contactRequirement);
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
            context.Succeed(contactRequirement);
        }
    }
}

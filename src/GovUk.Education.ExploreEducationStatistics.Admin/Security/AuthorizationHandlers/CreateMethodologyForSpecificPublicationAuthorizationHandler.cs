#nullable enable
using GovUk.Education.ExploreEducationStatistics.Common.Services.Security;
using GovUk.Education.ExploreEducationStatistics.Content.Model;
using GovUk.Education.ExploreEducationStatistics.Content.Model.Database;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;

namespace GovUk.Education.ExploreEducationStatistics.Admin.Security.AuthorizationHandlers;

public class CreateMethodologyForSpecificPublicationRequirement : IAuthorizationRequirement { }

public class CreateMethodologyForSpecificPublicationAuthorizationHandler(
    ContentDbContext contentDbContext,
    IAuthorizationHandlerService authorizationHandlerService
) : AuthorizationHandler<CreateMethodologyForSpecificPublicationRequirement, Publication>
{
    protected override async Task HandleRequirementAsync(
        AuthorizationHandlerContext context,
        CreateMethodologyForSpecificPublicationRequirement requirement,
        Publication publication
    )
    {
        // No user is allowed to create a new methodology of an archived or to-be-archived publication
        if (publication.SupersededById.HasValue)
        {
            return;
        }

        // If a publication owns a methodology already, they cannot own another
        if (
            await contentDbContext.PublicationMethodologies.AnyAsync(pm =>
                pm.PublicationId == publication.Id && pm.Owner
            )
        )
        {
            return;
        }

        if (SecurityUtils.HasClaim(context.User, SecurityClaimTypes.CreateAnyMethodology))
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

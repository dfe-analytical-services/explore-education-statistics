#nullable enable
using GovUk.Education.ExploreEducationStatistics.Common.Services.Security;
using GovUk.Education.ExploreEducationStatistics.Content.Model;
using Microsoft.AspNetCore.Authorization;

namespace GovUk.Education.ExploreEducationStatistics.Admin.Security.AuthorizationHandlers;

public class UpdatePublicationSummaryRequirement : IAuthorizationRequirement { }

public class UpdatePublicationSummaryAuthorizationHandler(IAuthorizationHandlerService authorizationHandlerService)
    : AuthorizationHandler<UpdatePublicationSummaryRequirement, Publication>
{
    protected override async Task HandleRequirementAsync(
        AuthorizationHandlerContext context,
        UpdatePublicationSummaryRequirement summaryRequirement,
        Publication publication
    )
    {
        if (SecurityUtils.HasClaim(context.User, SecurityClaimTypes.UpdateAllPublications))
        {
            context.Succeed(summaryRequirement);
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
            context.Succeed(summaryRequirement);
        }
    }
}

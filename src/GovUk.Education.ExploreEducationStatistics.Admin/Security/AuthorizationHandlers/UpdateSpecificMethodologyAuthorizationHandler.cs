#nullable enable
using GovUk.Education.ExploreEducationStatistics.Common.Services.Security;
using GovUk.Education.ExploreEducationStatistics.Content.Model;
using GovUk.Education.ExploreEducationStatistics.Content.Model.Repository.Interfaces;
using Microsoft.AspNetCore.Authorization;

namespace GovUk.Education.ExploreEducationStatistics.Admin.Security.AuthorizationHandlers;

public class UpdateSpecificMethodologyRequirement : IAuthorizationRequirement { }

public class UpdateSpecificMethodologyAuthorizationHandler(
    IMethodologyRepository methodologyRepository,
    IAuthorizationHandlerService authorizationHandlerService
) : AuthorizationHandler<UpdateSpecificMethodologyRequirement, MethodologyVersion>
{
    protected override async Task HandleRequirementAsync(
        AuthorizationHandlerContext context,
        UpdateSpecificMethodologyRequirement requirement,
        MethodologyVersion methodologyVersion
    )
    {
        if (methodologyVersion.Approved)
        {
            return;
        }

        if (SecurityUtils.HasClaim(context.User, SecurityClaimTypes.UpdateAllMethodologies))
        {
            context.Succeed(requirement);
            return;
        }

        var owningPublication = await methodologyRepository.GetOwningPublication(methodologyVersion.MethodologyId);

        if (
            await authorizationHandlerService.UserHasAnyPublicationRoleOnPublication(
                userId: context.User.GetUserId(),
                publicationId: owningPublication.Id,
                rolesToInclude: [PublicationRole.Drafter, PublicationRole.Approver]
            )
        )
        {
            context.Succeed(requirement);
        }
    }
}

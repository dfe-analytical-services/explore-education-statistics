#nullable enable
using GovUk.Education.ExploreEducationStatistics.Common.Services.Security;
using GovUk.Education.ExploreEducationStatistics.Content.Model;
using GovUk.Education.ExploreEducationStatistics.Content.Model.Repository.Interfaces;
using Microsoft.AspNetCore.Authorization;
using static GovUk.Education.ExploreEducationStatistics.Content.Model.MethodologyApprovalStatus;

namespace GovUk.Education.ExploreEducationStatistics.Admin.Security.AuthorizationHandlers;

public class DeleteSpecificMethodologyRequirement : IAuthorizationRequirement { }

public class DeleteSpecificMethodologyAuthorizationHandler(
    IMethodologyRepository methodologyRepository,
    IAuthorizationHandlerService authorizationHandlerService
) : AuthorizationHandler<DeleteSpecificMethodologyRequirement, MethodologyVersion>
{
    protected override async Task HandleRequirementAsync(
        AuthorizationHandlerContext context,
        DeleteSpecificMethodologyRequirement requirement,
        MethodologyVersion methodologyVersion
    )
    {
        if (methodologyVersion.Status == Approved)
        {
            return;
        }

        if (SecurityUtils.HasClaim(context.User, SecurityClaimTypes.DeleteAllMethodologies))
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

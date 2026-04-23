#nullable enable
using GovUk.Education.ExploreEducationStatistics.Common.Services.Security;
using GovUk.Education.ExploreEducationStatistics.Content.Model;
using GovUk.Education.ExploreEducationStatistics.Content.Model.Repository.Interfaces;
using Microsoft.AspNetCore.Authorization;

namespace GovUk.Education.ExploreEducationStatistics.Admin.Security.AuthorizationHandlers;

// ReSharper disable once ClassNeverInstantiated.Global
public class MarkMethodologyAsApprovedRequirement : IAuthorizationRequirement { }

public class MarkMethodologyAsApprovedAuthorizationHandler(
    IMethodologyVersionRepository methodologyVersionRepository,
    IMethodologyRepository methodologyRepository,
    IAuthorizationHandlerService authorizationHandlerService
) : AuthorizationHandler<MarkMethodologyAsApprovedRequirement, MethodologyVersion>
{
    protected override async Task HandleRequirementAsync(
        AuthorizationHandlerContext context,
        MarkMethodologyAsApprovedRequirement requirement,
        MethodologyVersion methodologyVersion
    )
    {
        // If the Methodology is already public, it cannot be approved
        // An approved Methodology that isn't public can be approved to change attributes associated with approval
        if (await methodologyVersionRepository.IsLatestPublishedVersion(methodologyVersion))
        {
            return;
        }

        if (SecurityUtils.HasClaim(context.User, SecurityClaimTypes.ApproveAllMethodologies))
        {
            context.Succeed(requirement);
            return;
        }

        var owningPublication = await methodologyRepository.GetOwningPublication(methodologyVersion.MethodologyId);

        if (
            await authorizationHandlerService.UserHasAnyPublicationRoleOnPublication(
                userId: context.User.GetUserId(),
                publicationId: owningPublication.Id,
                rolesToInclude: [PublicationRole.Approver]
            )
        )
        {
            context.Succeed(requirement);
        }
    }
}

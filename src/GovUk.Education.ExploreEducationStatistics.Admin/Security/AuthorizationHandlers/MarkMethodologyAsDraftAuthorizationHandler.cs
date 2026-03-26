#nullable enable
using GovUk.Education.ExploreEducationStatistics.Common.Services.Security;
using GovUk.Education.ExploreEducationStatistics.Content.Model;
using GovUk.Education.ExploreEducationStatistics.Content.Model.Repository.Interfaces;
using Microsoft.AspNetCore.Authorization;
using static GovUk.Education.ExploreEducationStatistics.Common.Services.CollectionUtils;
using static GovUk.Education.ExploreEducationStatistics.Content.Model.MethodologyApprovalStatus;

namespace GovUk.Education.ExploreEducationStatistics.Admin.Security.AuthorizationHandlers;

// ReSharper disable once ClassNeverInstantiated.Global
public class MarkMethodologyAsDraftRequirement : IAuthorizationRequirement { }

public class MarkMethodologyAsDraftAuthorizationHandler(
    IMethodologyVersionRepository methodologyVersionRepository,
    IMethodologyRepository methodologyRepository,
    IAuthorizationHandlerService authorizationHandlerService
) : AuthorizationHandler<MarkMethodologyAsDraftRequirement, MethodologyVersion>
{
    protected override async Task HandleRequirementAsync(
        AuthorizationHandlerContext context,
        MarkMethodologyAsDraftRequirement requirement,
        MethodologyVersion methodologyVersion
    )
    {
        // If the Methodology is already public, it cannot be marked as draft
        if (await methodologyVersionRepository.IsLatestPublishedVersion(methodologyVersion))
        {
            return;
        }

        if (SecurityUtils.HasClaim(context.User, SecurityClaimTypes.MarkAllMethodologiesDraft))
        {
            context.Succeed(requirement);
            return;
        }

        var allowedPublicationRoles =
            methodologyVersion.Status == Approved
                ? SetOf(PublicationRole.Approver)
                : SetOf(PublicationRole.Drafter, PublicationRole.Approver);

        var owningPublication = await methodologyRepository.GetOwningPublication(methodologyVersion.MethodologyId);

        if (
            await authorizationHandlerService.UserHasAnyPublicationRoleOnPublication(
                userId: context.User.GetUserId(),
                publicationId: owningPublication.Id,
                rolesToInclude: allowedPublicationRoles
            )
        )
        {
            context.Succeed(requirement);
        }
    }
}

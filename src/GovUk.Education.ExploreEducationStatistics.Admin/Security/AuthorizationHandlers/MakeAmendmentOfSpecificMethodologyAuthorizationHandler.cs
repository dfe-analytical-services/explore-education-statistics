#nullable enable
using GovUk.Education.ExploreEducationStatistics.Common.Services.Security;
using GovUk.Education.ExploreEducationStatistics.Content.Model;
using GovUk.Education.ExploreEducationStatistics.Content.Model.Repository.Interfaces;
using Microsoft.AspNetCore.Authorization;

namespace GovUk.Education.ExploreEducationStatistics.Admin.Security.AuthorizationHandlers;

public class MakeAmendmentOfSpecificMethodologyRequirement : IAuthorizationRequirement { }

public class MakeAmendmentOfSpecificMethodologyAuthorizationHandler(
    IMethodologyVersionRepository methodologyVersionRepository,
    IMethodologyRepository methodologyRepository,
    IAuthorizationHandlerService authorizationHandlerService
) : AuthorizationHandler<MakeAmendmentOfSpecificMethodologyRequirement, MethodologyVersion>
{
    protected override async Task HandleRequirementAsync(
        AuthorizationHandlerContext context,
        MakeAmendmentOfSpecificMethodologyRequirement requirement,
        MethodologyVersion methodologyVersion
    )
    {
        // Amendments can only be created from Methodologies that are already publicly-accessible.
        if (!await methodologyVersionRepository.IsLatestPublishedVersion(methodologyVersion))
        {
            return;
        }

        // Any user with the "MakeAmendmentsOfAllMethodologies" Claim can create an amendment of a
        // publicly-accessible Methodology.
        if (SecurityUtils.HasClaim(context.User, SecurityClaimTypes.MakeAmendmentsOfAllMethodologies))
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

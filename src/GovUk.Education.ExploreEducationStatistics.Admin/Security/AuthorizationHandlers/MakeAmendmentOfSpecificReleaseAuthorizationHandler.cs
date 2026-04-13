#nullable enable
using GovUk.Education.ExploreEducationStatistics.Common.Services.Security;
using GovUk.Education.ExploreEducationStatistics.Content.Model;
using GovUk.Education.ExploreEducationStatistics.Content.Model.Repository.Interfaces;
using Microsoft.AspNetCore.Authorization;

namespace GovUk.Education.ExploreEducationStatistics.Admin.Security.AuthorizationHandlers;

public class MakeAmendmentOfSpecificReleaseRequirement : IAuthorizationRequirement { }

public class MakeAmendmentOfSpecificReleaseAuthorizationHandler(
    IReleaseVersionRepository releaseVersionRepository,
    IAuthorizationHandlerService authorizationHandlerService
) : AuthorizationHandler<MakeAmendmentOfSpecificReleaseRequirement, ReleaseVersion>
{
    protected override async Task HandleRequirementAsync(
        AuthorizationHandlerContext context,
        MakeAmendmentOfSpecificReleaseRequirement requirement,
        ReleaseVersion releaseVersion
    )
    {
        if (!releaseVersion.Live)
        {
            return;
        }

        if (!await releaseVersionRepository.IsLatestReleaseVersion(releaseVersion.Id))
        {
            return;
        }

        if (SecurityUtils.HasClaim(context.User, SecurityClaimTypes.MakeAmendmentsOfAllReleases))
        {
            context.Succeed(requirement);
            return;
        }

        if (
            await authorizationHandlerService.UserHasAnyPublicationRoleOnPublication(
                userId: context.User.GetUserId(),
                publicationId: releaseVersion.Release.PublicationId,
                rolesToInclude: [PublicationRole.Drafter, PublicationRole.Approver]
            )
        )
        {
            context.Succeed(requirement);
        }
    }
}

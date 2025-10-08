#nullable enable
using GovUk.Education.ExploreEducationStatistics.Common.Services.Security;
using GovUk.Education.ExploreEducationStatistics.Content.Model;
using GovUk.Education.ExploreEducationStatistics.Content.Model.Repository.Interfaces;
using Microsoft.AspNetCore.Authorization;
using static GovUk.Education.ExploreEducationStatistics.Admin.Security.SecurityClaimTypes;
using static GovUk.Education.ExploreEducationStatistics.Content.Model.PublicationRole;

namespace GovUk.Education.ExploreEducationStatistics.Admin.Security.AuthorizationHandlers;

public class MakeAmendmentOfSpecificReleaseRequirement : IAuthorizationRequirement { }

public class MakeAmendmentOfSpecificReleaseAuthorizationHandler
    : AuthorizationHandler<MakeAmendmentOfSpecificReleaseRequirement, ReleaseVersion>
{
    private readonly AuthorizationHandlerService _authorizationHandlerService;
    private readonly IReleaseVersionRepository _releaseVersionRepository;

    public MakeAmendmentOfSpecificReleaseAuthorizationHandler(
        AuthorizationHandlerService authorizationHandlerService,
        IReleaseVersionRepository releaseVersionRepository
    )
    {
        _authorizationHandlerService = authorizationHandlerService;
        _releaseVersionRepository = releaseVersionRepository;
    }

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

        if (!await _releaseVersionRepository.IsLatestReleaseVersion(releaseVersion.Id))
        {
            return;
        }

        if (SecurityUtils.HasClaim(context.User, MakeAmendmentsOfAllReleases))
        {
            context.Succeed(requirement);
            return;
        }

        if (
            await _authorizationHandlerService.HasRolesOnPublication(
                context.User.GetUserId(),
                releaseVersion.PublicationId,
                Owner
            )
        )
        {
            context.Succeed(requirement);
        }
    }
}

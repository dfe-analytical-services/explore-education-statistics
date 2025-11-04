#nullable enable
using GovUk.Education.ExploreEducationStatistics.Content.Model;
using GovUk.Education.ExploreEducationStatistics.Content.Model.Repository.Interfaces;
using Microsoft.AspNetCore.Authorization;

namespace GovUk.Education.ExploreEducationStatistics.Content.Security.AuthorizationHandlers;

public class ViewReleaseRequirement : IAuthorizationRequirement { }

public class ViewReleaseAuthorizationHandler : AuthorizationHandler<ViewReleaseRequirement, ReleaseVersion>
{
    private readonly IReleaseVersionRepository _releaseVersionRepository;

    public ViewReleaseAuthorizationHandler(IReleaseVersionRepository releaseVersionRepository)
    {
        _releaseVersionRepository = releaseVersionRepository;
    }

    protected override async Task HandleRequirementAsync(
        AuthorizationHandlerContext authContext,
        ViewReleaseRequirement requirement,
        ReleaseVersion releaseVersion
    )
    {
        if (await _releaseVersionRepository.IsLatestPublishedReleaseVersion(releaseVersion.Id))
        {
            authContext.Succeed(requirement);
        }
    }
}

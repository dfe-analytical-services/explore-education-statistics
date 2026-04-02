#nullable enable
using GovUk.Education.ExploreEducationStatistics.Content.Model;
using GovUk.Education.ExploreEducationStatistics.Content.Model.Repository.Interfaces;
using Microsoft.AspNetCore.Authorization;

namespace GovUk.Education.ExploreEducationStatistics.Content.Security.AuthorizationHandlers;

public class ViewReleaseVersionRequirement : IAuthorizationRequirement { }

public class ViewReleaseVersionAuthorizationHandler
    : AuthorizationHandler<ViewReleaseVersionRequirement, ReleaseVersion>
{
    private readonly IReleaseVersionRepository _releaseVersionRepository;

    public ViewReleaseVersionAuthorizationHandler(IReleaseVersionRepository releaseVersionRepository)
    {
        _releaseVersionRepository = releaseVersionRepository;
    }

    protected override async Task HandleRequirementAsync(
        AuthorizationHandlerContext authContext,
        ViewReleaseVersionRequirement requirement,
        ReleaseVersion releaseVersion
    )
    {
        if (await _releaseVersionRepository.IsLatestPublishedReleaseVersion(releaseVersion.Id))
        {
            authContext.Succeed(requirement);
        }
    }
}

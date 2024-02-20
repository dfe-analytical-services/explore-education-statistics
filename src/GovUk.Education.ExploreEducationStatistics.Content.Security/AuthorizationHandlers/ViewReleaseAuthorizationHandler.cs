#nullable enable
using System.Threading.Tasks;
using GovUk.Education.ExploreEducationStatistics.Content.Model;
using GovUk.Education.ExploreEducationStatistics.Content.Model.Repository.Interfaces;
using Microsoft.AspNetCore.Authorization;

namespace GovUk.Education.ExploreEducationStatistics.Content.Security.AuthorizationHandlers;

public class ViewReleaseRequirement : IAuthorizationRequirement
{
}

public class ViewReleaseAuthorizationHandler
    : AuthorizationHandler<ViewReleaseRequirement, ReleaseVersion>
{
    private readonly IReleaseRepository _releaseRepository;

    public ViewReleaseAuthorizationHandler(IReleaseRepository releaseRepository)
    {
        _releaseRepository = releaseRepository;
    }

    protected override async Task HandleRequirementAsync(
        AuthorizationHandlerContext authContext,
        ViewReleaseRequirement requirement,
        ReleaseVersion releaseVersion)
    {
        if (await _releaseRepository.IsLatestPublishedReleaseVersion(releaseVersion.Id))
        {
            authContext.Succeed(requirement);
        }
    }
}

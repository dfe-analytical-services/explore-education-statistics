#nullable enable
using GovUk.Education.ExploreEducationStatistics.Content.Model;
using GovUk.Education.ExploreEducationStatistics.Content.Model.Repository.Interfaces;
using Microsoft.AspNetCore.Authorization;

namespace GovUk.Education.ExploreEducationStatistics.Data.Services.Security.AuthorizationHandlers;

public class ViewSubjectDataForPublishedReleasesAuthorizationHandler
    : AuthorizationHandler<ViewSubjectDataRequirement, ReleaseFile>
{
    private readonly IReleaseVersionRepository _releaseVersionRepository;

    public ViewSubjectDataForPublishedReleasesAuthorizationHandler(IReleaseVersionRepository releaseVersionRepository)
    {
        _releaseVersionRepository = releaseVersionRepository;
    }

    protected override async Task HandleRequirementAsync(
        AuthorizationHandlerContext authContext,
        ViewSubjectDataRequirement requirement,
        ReleaseFile releaseFile
    )
    {
        if (await _releaseVersionRepository.IsLatestPublishedReleaseVersion(releaseFile.ReleaseVersionId))
        {
            authContext.Succeed(requirement);
        }
    }
}

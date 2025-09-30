#nullable enable
using GovUk.Education.ExploreEducationStatistics.Content.Model.Repository.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Data.Model;
using Microsoft.AspNetCore.Authorization;

namespace GovUk.Education.ExploreEducationStatistics.Data.Services.Security.AuthorizationHandlers;

public class ViewSubjectDataForPublishedReleasesAuthorizationHandler
    : AuthorizationHandler<ViewSubjectDataRequirement, ReleaseSubject>
{
    private readonly IReleaseVersionRepository _releaseVersionRepository;

    public ViewSubjectDataForPublishedReleasesAuthorizationHandler(
        IReleaseVersionRepository releaseVersionRepository
    )
    {
        _releaseVersionRepository = releaseVersionRepository;
    }

    protected override async Task HandleRequirementAsync(
        AuthorizationHandlerContext authContext,
        ViewSubjectDataRequirement requirement,
        ReleaseSubject releaseSubject
    )
    {
        if (
            await _releaseVersionRepository.IsLatestPublishedReleaseVersion(
                releaseSubject.ReleaseVersionId
            )
        )
        {
            authContext.Succeed(requirement);
        }
    }
}

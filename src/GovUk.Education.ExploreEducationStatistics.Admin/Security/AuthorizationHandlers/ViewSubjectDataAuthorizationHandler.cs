#nullable enable
using GovUk.Education.ExploreEducationStatistics.Content.Model;
using GovUk.Education.ExploreEducationStatistics.Content.Model.Database;
using GovUk.Education.ExploreEducationStatistics.Data.Services.Security;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;

namespace GovUk.Education.ExploreEducationStatistics.Admin.Security.AuthorizationHandlers;

public class ViewSubjectDataAuthorizationHandler(
    ContentDbContext contentDbContext,
    IAuthorizationHandlerService authorizationHandlerService
) : AuthorizationHandler<ViewSubjectDataRequirement, ReleaseFile>
{
    protected override async Task HandleRequirementAsync(
        AuthorizationHandlerContext context,
        ViewSubjectDataRequirement requirement,
        ReleaseFile releaseFile
    )
    {
        // If this data has been published, it is visible to anyone.
        var releaseVersion = await contentDbContext
            .ReleaseVersions.Include(rv => rv.Release)
            .SingleAsync(rv => rv.Id == releaseFile.ReleaseVersionId);

        if (await authorizationHandlerService.IsReleaseVersionViewableByUser(releaseVersion, context.User))
        {
            context.Succeed(requirement);
        }
    }
}

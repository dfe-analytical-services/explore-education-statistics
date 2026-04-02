#nullable enable
using GovUk.Education.ExploreEducationStatistics.Content.Model.Database;
using GovUk.Education.ExploreEducationStatistics.Data.Model;
using GovUk.Education.ExploreEducationStatistics.Data.Services.Security;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;

namespace GovUk.Education.ExploreEducationStatistics.Admin.Security.AuthorizationHandlers;

public class ViewSubjectDataAuthorizationHandler(
    ContentDbContext contentDbContext,
    IAuthorizationHandlerService authorizationHandlerService
) : AuthorizationHandler<ViewSubjectDataRequirement, ReleaseSubject>
{
    protected override async Task HandleRequirementAsync(
        AuthorizationHandlerContext context,
        ViewSubjectDataRequirement requirement,
        ReleaseSubject releaseSubject
    )
    {
        // If this data has been published, it is visible to anyone.
        var releaseVersion = await contentDbContext.ReleaseVersions.SingleAsync(rv =>
            rv.Id == releaseSubject.ReleaseVersionId
        );

        if (await authorizationHandlerService.IsReleaseVersionViewableByUser(releaseVersion, context.User))
        {
            context.Succeed(requirement);
        }
    }
}

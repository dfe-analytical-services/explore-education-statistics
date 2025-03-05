#nullable enable
using System.Threading.Tasks;
using GovUk.Education.ExploreEducationStatistics.Content.Model.Database;
using GovUk.Education.ExploreEducationStatistics.Data.Model;
using GovUk.Education.ExploreEducationStatistics.Data.Services.Security;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;

namespace GovUk.Education.ExploreEducationStatistics.Admin.Security.AuthorizationHandlers;

public class ViewSubjectDataAuthorizationHandler : AuthorizationHandler<
    ViewSubjectDataRequirement, ReleaseSubject>
{
    private readonly ContentDbContext _contentDbContext;
    private readonly AuthorizationHandlerService _authorizationHandlerService;

    public ViewSubjectDataAuthorizationHandler(
        ContentDbContext contentDbContext,
        AuthorizationHandlerService authorizationHandlerService)
    {
        _contentDbContext = contentDbContext;
        _authorizationHandlerService = authorizationHandlerService;
    }

    protected override async Task HandleRequirementAsync(
        AuthorizationHandlerContext context,
        ViewSubjectDataRequirement requirement,
        ReleaseSubject releaseSubject)
    {
        // If this data has been published, it is visible to anyone.
        var releaseVersion = await _contentDbContext
            .ReleaseVersions
            .FirstAsync(rv => rv.Id == releaseSubject.ReleaseVersionId);

        if (await _authorizationHandlerService.IsReleaseVersionViewableByUser(releaseVersion, context.User))
        {
            context.Succeed(requirement);
        }
    }
}

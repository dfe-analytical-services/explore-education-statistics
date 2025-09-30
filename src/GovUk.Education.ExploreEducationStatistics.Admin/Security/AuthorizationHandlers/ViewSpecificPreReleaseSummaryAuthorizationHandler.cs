#nullable enable
using GovUk.Education.ExploreEducationStatistics.Common.Services.Security;
using GovUk.Education.ExploreEducationStatistics.Content.Model;
using Microsoft.AspNetCore.Authorization;
using static GovUk.Education.ExploreEducationStatistics.Admin.Security.AuthorizationHandlers.AuthorizationHandlerService;
using static GovUk.Education.ExploreEducationStatistics.Admin.Security.SecurityClaimTypes;
using static GovUk.Education.ExploreEducationStatistics.Common.Services.CollectionUtils;

namespace GovUk.Education.ExploreEducationStatistics.Admin.Security.AuthorizationHandlers;

public class ViewSpecificPreReleaseSummaryRequirement : IAuthorizationRequirement { }

public class ViewSpecificPreReleaseSummaryAuthorizationHandler
    : AuthorizationHandler<ViewSpecificPreReleaseSummaryRequirement, ReleaseVersion>
{
    private readonly AuthorizationHandlerService _authorizationHandlerService;

    private static readonly ReleaseRole[] UnrestrictedReleaseViewerAndPrereleaseViewerRoles =
        UnrestrictedReleaseViewerRoles.Append(ReleaseRole.PrereleaseViewer).ToArray();

    public ViewSpecificPreReleaseSummaryAuthorizationHandler(
        AuthorizationHandlerService authorizationHandlerService
    )
    {
        _authorizationHandlerService = authorizationHandlerService;
    }

    protected override async Task HandleRequirementAsync(
        AuthorizationHandlerContext context,
        ViewSpecificPreReleaseSummaryRequirement requirement,
        ReleaseVersion releaseVersion
    )
    {
        if (SecurityUtils.HasClaim(context.User, AccessAllReleases))
        {
            context.Succeed(requirement);
            return;
        }

        if (
            await _authorizationHandlerService.HasRolesOnPublicationOrReleaseVersion(
                context.User.GetUserId(),
                releaseVersion.PublicationId,
                releaseVersion.Id,
                ListOf(PublicationRole.Owner, PublicationRole.Allower),
                UnrestrictedReleaseViewerAndPrereleaseViewerRoles
            )
        )
        {
            context.Succeed(requirement);
        }
    }
}

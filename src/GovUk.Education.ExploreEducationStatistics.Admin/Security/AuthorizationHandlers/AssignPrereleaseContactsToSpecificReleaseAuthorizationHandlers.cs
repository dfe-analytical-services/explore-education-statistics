using GovUk.Education.ExploreEducationStatistics.Common.Services.Security;
using GovUk.Education.ExploreEducationStatistics.Content.Model;
using Microsoft.AspNetCore.Authorization;
using static GovUk.Education.ExploreEducationStatistics.Admin.Security.AuthorizationHandlers.AuthorizationHandlerService;
using static GovUk.Education.ExploreEducationStatistics.Admin.Security.SecurityClaimTypes;
using static GovUk.Education.ExploreEducationStatistics.Common.Services.CollectionUtils;

namespace GovUk.Education.ExploreEducationStatistics.Admin.Security.AuthorizationHandlers;

public class AssignPrereleaseContactsToSpecificReleaseRequirement : IAuthorizationRequirement
{
}

public class AssignPrereleaseContactsToSpecificReleaseAuthorizationHandler
    : AuthorizationHandler<AssignPrereleaseContactsToSpecificReleaseRequirement, ReleaseVersion>
{
    private readonly AuthorizationHandlerService _authorizationHandlerService;

    public AssignPrereleaseContactsToSpecificReleaseAuthorizationHandler(
        AuthorizationHandlerService authorizationHandlerService)
    {
        _authorizationHandlerService = authorizationHandlerService;
    }

    protected override async Task HandleRequirementAsync(
        AuthorizationHandlerContext context,
        AssignPrereleaseContactsToSpecificReleaseRequirement requirement,
        ReleaseVersion releaseVersion)
    {
        if (SecurityUtils.HasClaim(context.User, UpdateAllReleases))
        {
            context.Succeed(requirement);
            return;
        }

        if (await _authorizationHandlerService
                .HasRolesOnPublicationOrReleaseVersion(
                    context.User.GetUserId(),
                    releaseVersion.PublicationId,
                    releaseVersion.Id,
                    ListOf(PublicationRole.Owner, PublicationRole.Allower),
                    ReleaseEditorAndApproverRoles))
        {
            context.Succeed(requirement);
        }
    }
}

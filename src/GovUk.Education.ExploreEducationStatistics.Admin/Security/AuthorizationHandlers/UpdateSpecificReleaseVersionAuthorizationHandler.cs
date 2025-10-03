using GovUk.Education.ExploreEducationStatistics.Common.Services.Security;
using GovUk.Education.ExploreEducationStatistics.Content.Model;
using Microsoft.AspNetCore.Authorization;
using static GovUk.Education.ExploreEducationStatistics.Admin.Security.AuthorizationHandlers.AuthorizationHandlerService;
using static GovUk.Education.ExploreEducationStatistics.Common.Services.CollectionUtils;
using static GovUk.Education.ExploreEducationStatistics.Content.Model.ReleaseApprovalStatus;

namespace GovUk.Education.ExploreEducationStatistics.Admin.Security.AuthorizationHandlers;

public class UpdateSpecificReleaseVersionRequirement : IAuthorizationRequirement;

public class UpdateSpecificReleaseVersionAuthorizationHandler
    : AuthorizationHandler<UpdateSpecificReleaseVersionRequirement, ReleaseVersion>
{
    private readonly AuthorizationHandlerService _authorizationHandlerService;

    public UpdateSpecificReleaseVersionAuthorizationHandler(AuthorizationHandlerService authorizationHandlerService)
    {
        _authorizationHandlerService = authorizationHandlerService;
    }

    protected override async Task HandleRequirementAsync(
        AuthorizationHandlerContext context,
        UpdateSpecificReleaseVersionRequirement requirement,
        ReleaseVersion releaseVersion
    )
    {
        if (releaseVersion.ApprovalStatus == Approved)
        {
            return;
        }

        if (SecurityUtils.HasClaim(context.User, SecurityClaimTypes.UpdateAllReleases))
        {
            context.Succeed(requirement);
            return;
        }

        var allowedPublicationRoles = ListOf(PublicationRole.Owner, PublicationRole.Allower);
        var allowedReleaseRoles = ReleaseEditorAndApproverRoles;

        if (
            await _authorizationHandlerService.HasRolesOnPublicationOrReleaseVersion(
                context.User.GetUserId(),
                releaseVersion.PublicationId,
                releaseVersion.Id,
                allowedPublicationRoles,
                allowedReleaseRoles
            )
        )
        {
            context.Succeed(requirement);
        }
    }
}

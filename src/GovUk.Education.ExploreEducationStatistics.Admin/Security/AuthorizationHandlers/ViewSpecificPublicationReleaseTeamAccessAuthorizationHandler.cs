#nullable enable
using GovUk.Education.ExploreEducationStatistics.Common.Services.Security;
using GovUk.Education.ExploreEducationStatistics.Content.Model;
using Microsoft.AspNetCore.Authorization;
using static GovUk.Education.ExploreEducationStatistics.Admin.Security.SecurityClaimTypes;

namespace GovUk.Education.ExploreEducationStatistics.Admin.Security.AuthorizationHandlers;

public class ViewSpecificPublicationReleaseTeamAccessRequirement : IAuthorizationRequirement
{
}

public class ViewSpecificPublicationReleaseTeamAccessAuthorizationHandler :
    AuthorizationHandler<ViewSpecificPublicationReleaseTeamAccessRequirement, Publication>
{
    private readonly AuthorizationHandlerService _authorizationHandlerService;

    public ViewSpecificPublicationReleaseTeamAccessAuthorizationHandler(
        AuthorizationHandlerService authorizationHandlerService)
    {
        _authorizationHandlerService = authorizationHandlerService;
    }

    protected override async Task HandleRequirementAsync(
        AuthorizationHandlerContext context,
        ViewSpecificPublicationReleaseTeamAccessRequirement requirement,
        Publication publication)
    {
        if (SecurityUtils.HasClaim(context.User, AccessAllPublications))
        {
            context.Succeed(requirement);
            return;
        }

        if (await _authorizationHandlerService
                .HasRolesOnPublication(
                    context.User.GetUserId(),
                    publication.Id,
                    PublicationRole.Owner, PublicationRole.Allower))
        {
            context.Succeed(requirement);
        }
    }
}

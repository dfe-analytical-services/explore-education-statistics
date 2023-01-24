#nullable enable
using System.Threading.Tasks;
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
    private readonly AuthorizationHandlerResourceRoleService _authorizationHandlerResourceRoleService;

    public ViewSpecificPublicationReleaseTeamAccessAuthorizationHandler(
        AuthorizationHandlerResourceRoleService authorizationHandlerResourceRoleService)
    {
        _authorizationHandlerResourceRoleService = authorizationHandlerResourceRoleService;
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

        if (await _authorizationHandlerResourceRoleService
                .HasRolesOnPublication(
                    context.User.GetUserId(),
                    publication.Id,
                    PublicationRole.Owner, PublicationRole.Approver))
        {
            context.Succeed(requirement);
        }
    }
}
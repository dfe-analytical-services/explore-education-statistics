#nullable enable
using GovUk.Education.ExploreEducationStatistics.Common.Services.Security;
using GovUk.Education.ExploreEducationStatistics.Content.Model;
using Microsoft.AspNetCore.Authorization;
using static GovUk.Education.ExploreEducationStatistics.Content.Model.PublicationRole;

namespace GovUk.Education.ExploreEducationStatistics.Admin.Security.AuthorizationHandlers;

public class AdoptMethodologyForSpecificPublicationRequirement : IAuthorizationRequirement
{
}

public class AdoptMethodologyForSpecificPublicationAuthorizationHandler
    : AuthorizationHandler<AdoptMethodologyForSpecificPublicationRequirement, Publication>
{
    private readonly AuthorizationHandlerService _authorizationHandlerService;

    public AdoptMethodologyForSpecificPublicationAuthorizationHandler(
        AuthorizationHandlerService authorizationHandlerService)
    {
        _authorizationHandlerService = authorizationHandlerService;
    }

    protected override async Task HandleRequirementAsync(
        AuthorizationHandlerContext context,
        AdoptMethodologyForSpecificPublicationRequirement requirement,
        Publication publication)
    {
        if (SecurityUtils.HasClaim(context.User, SecurityClaimTypes.AdoptAnyMethodology))
        {
            context.Succeed(requirement);
            return;
        }
        
        if (await _authorizationHandlerService
                .HasRolesOnPublication(
                    context.User.GetUserId(),
                    publication.Id,
                    Owner))
        {
            context.Succeed(requirement);
        }
    }
}

#nullable enable
using GovUk.Education.ExploreEducationStatistics.Common.Services.Security;
using GovUk.Education.ExploreEducationStatistics.Content.Model;
using Microsoft.AspNetCore.Authorization;
using static GovUk.Education.ExploreEducationStatistics.Admin.Security.SecurityClaimTypes;
using static GovUk.Education.ExploreEducationStatistics.Content.Model.PublicationRole;

namespace GovUk.Education.ExploreEducationStatistics.Admin.Security.AuthorizationHandlers;

public class ManageExternalMethodologyForSpecificPublicationRequirement : IAuthorizationRequirement
{
}

public class ManageExternalMethodologyForSpecificPublicationAuthorizationHandler 
    : AuthorizationHandler<ManageExternalMethodologyForSpecificPublicationRequirement, Publication>
{
    private readonly AuthorizationHandlerService _authorizationHandlerService;

    public ManageExternalMethodologyForSpecificPublicationAuthorizationHandler(
        AuthorizationHandlerService authorizationHandlerService)
    {
        _authorizationHandlerService = authorizationHandlerService;
    }

    protected override async Task HandleRequirementAsync(AuthorizationHandlerContext context,
        ManageExternalMethodologyForSpecificPublicationRequirement requirement,
        Publication publication)
    {
        if (SecurityUtils.HasClaim(context.User, CreateAnyMethodology))
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

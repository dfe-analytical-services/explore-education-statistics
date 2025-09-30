#nullable enable
using GovUk.Education.ExploreEducationStatistics.Common.Services.Security;
using GovUk.Education.ExploreEducationStatistics.Content.Model;
using Microsoft.AspNetCore.Authorization;
using static GovUk.Education.ExploreEducationStatistics.Admin.Security.SecurityClaimTypes;
using static GovUk.Education.ExploreEducationStatistics.Content.Model.PublicationRole;

namespace GovUk.Education.ExploreEducationStatistics.Admin.Security.AuthorizationHandlers;

public class DropMethodologyLinkRequirement : IAuthorizationRequirement
{
}

public class DropMethodologyLinkAuthorizationHandler
    : AuthorizationHandler<DropMethodologyLinkRequirement, PublicationMethodology>
{
    private readonly AuthorizationHandlerService _authorizationHandlerService;

    public DropMethodologyLinkAuthorizationHandler(
        AuthorizationHandlerService authorizationHandlerService)
    {
        _authorizationHandlerService = authorizationHandlerService;
    }

    protected override async Task HandleRequirementAsync(
        AuthorizationHandlerContext context,
        DropMethodologyLinkRequirement requirement,
        PublicationMethodology link)
    {
        if (link.Owner)
        {
            // No user is allowed to drop the link between a methodology and its owning publication 
            return;
        }

        // Allow users who can adopt methodologies to also drop them
        if (SecurityUtils.HasClaim(context.User, AdoptAnyMethodology))
        {
            context.Succeed(requirement);
            return;
        }

        if (await _authorizationHandlerService
                .HasRolesOnPublication(
                    context.User.GetUserId(),
                    link.PublicationId,
                    Owner))
        {
            context.Succeed(requirement);
        }
    }
}

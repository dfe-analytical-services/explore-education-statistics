#nullable enable
using GovUk.Education.ExploreEducationStatistics.Common.Services.Security;
using GovUk.Education.ExploreEducationStatistics.Content.Model;
using Microsoft.AspNetCore.Authorization;
using static GovUk.Education.ExploreEducationStatistics.Admin.Security.SecurityClaimTypes;
using static GovUk.Education.ExploreEducationStatistics.Content.Model.PublicationRole;

namespace GovUk.Education.ExploreEducationStatistics.Admin.Security.AuthorizationHandlers;

public class CreateReleaseForSpecificPublicationRequirement : IAuthorizationRequirement
{
}

public class CreateReleaseForSpecificPublicationAuthorizationHandler
    : AuthorizationHandler<CreateReleaseForSpecificPublicationRequirement, Publication>
{
    private readonly AuthorizationHandlerService _authorizationHandlerService;

    public CreateReleaseForSpecificPublicationAuthorizationHandler(
        AuthorizationHandlerService authorizationHandlerService)
    {
        _authorizationHandlerService = authorizationHandlerService;
    }

    protected override async Task HandleRequirementAsync(AuthorizationHandlerContext context,
        CreateReleaseForSpecificPublicationRequirement requirement,
        Publication publication)
    {
        // No user is allowed to create a new release of an archived publication
        if (publication.SupersededById.HasValue)
        {
            return;
        }

        if (SecurityUtils.HasClaim(context.User, CreateAnyRelease))
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

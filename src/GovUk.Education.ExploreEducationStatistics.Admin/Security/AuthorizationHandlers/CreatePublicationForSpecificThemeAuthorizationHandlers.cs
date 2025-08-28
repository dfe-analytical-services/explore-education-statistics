using GovUk.Education.ExploreEducationStatistics.Content.Model;
using Microsoft.AspNetCore.Authorization;

namespace GovUk.Education.ExploreEducationStatistics.Admin.Security.AuthorizationHandlers;

public class CreatePublicationForSpecificThemeRequirement : IAuthorizationRequirement
{ }

public class CreatePublicationForSpecificThemeAuthorizationHandler :
    AuthorizationHandler<CreatePublicationForSpecificThemeRequirement, Theme>
{
    protected override Task HandleRequirementAsync(AuthorizationHandlerContext context,
        CreatePublicationForSpecificThemeRequirement requirement, Theme resource)
    {
        if (SecurityUtils.HasClaim(context.User, SecurityClaimTypes.CreateAnyPublication))
        {
            context.Succeed(requirement);
        }

        return Task.CompletedTask;
    }
}

#nullable enable
using Microsoft.AspNetCore.Authorization;
using static GovUk.Education.ExploreEducationStatistics.Admin.Models.GlobalRoles;

namespace GovUk.Education.ExploreEducationStatistics.Admin.Security.AuthorizationHandlers;

public class ManagePublicApiDataSetsRequirement : IAuthorizationRequirement;

public class ManagePublicApiDataSetsAuthorizationHandler : AuthorizationHandler<ManagePublicApiDataSetsRequirement>
{
    // TODO Publication-specific roles will also be able to satisfy this requirement in future -
    // this handler will then need to take a resource (e.g. a publication id) to check against.
    protected override Task HandleRequirementAsync(
        AuthorizationHandlerContext context,
        ManagePublicApiDataSetsRequirement requirement
    )
    {
        if (context.User.IsInRole(RoleNames.BauUser))
        {
            context.Succeed(requirement);
        }

        return Task.CompletedTask;
    }
}

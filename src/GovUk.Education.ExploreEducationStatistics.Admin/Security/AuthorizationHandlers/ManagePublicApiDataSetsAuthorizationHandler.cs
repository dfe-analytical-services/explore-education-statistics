#nullable enable
using GovUk.Education.ExploreEducationStatistics.Admin.Extensions;
using GovUk.Education.ExploreEducationStatistics.Content.Model;
using Microsoft.AspNetCore.Authorization;

namespace GovUk.Education.ExploreEducationStatistics.Admin.Security.AuthorizationHandlers;

public class ManagePublicApiDataSetsRequirement : IAuthorizationRequirement;

public class ManagePublicApiDataSetsAuthorizationHandler
    : AuthorizationHandler<ManagePublicApiDataSetsRequirement, User>
{
    protected override async Task HandleRequirementAsync(
        AuthorizationHandlerContext context,
        ManagePublicApiDataSetsRequirement requirement,
        User user
    )
    {
        if (!user.Active)
        {
            return;
        }

        if (user.IsBau())
        {
            context.Succeed(requirement);
            return;
        }
    }
}

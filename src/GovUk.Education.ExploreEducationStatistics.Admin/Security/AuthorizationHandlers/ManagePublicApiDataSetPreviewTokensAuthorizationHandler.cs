#nullable enable
using GovUk.Education.ExploreEducationStatistics.Admin.Extensions;
using GovUk.Education.ExploreEducationStatistics.Content.Model;
using Microsoft.AspNetCore.Authorization;

namespace GovUk.Education.ExploreEducationStatistics.Admin.Security.AuthorizationHandlers;

public class ManagePublicApiDataSetPreviewTokensRequirement : IAuthorizationRequirement;

public class ManagePublicApiDataSetPreviewTokensAuthorizationHandler(
    IAuthorizationHandlerService authorizationHandlerService
) : AuthorizationHandler<ManagePublicApiDataSetPreviewTokensRequirement, User>
{
    protected override async Task HandleRequirementAsync(
        AuthorizationHandlerContext context,
        ManagePublicApiDataSetPreviewTokensRequirement requirement,
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

        if (await authorizationHandlerService.UserHasAnyPublicationRole(userId: user.Id))
        {
            context.Succeed(requirement);
        }
    }
}

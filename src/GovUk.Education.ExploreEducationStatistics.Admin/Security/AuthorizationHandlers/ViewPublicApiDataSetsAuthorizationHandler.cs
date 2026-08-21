#nullable enable
using GovUk.Education.ExploreEducationStatistics.Admin.Extensions;
using GovUk.Education.ExploreEducationStatistics.Content.Model;
using Microsoft.AspNetCore.Authorization;

namespace GovUk.Education.ExploreEducationStatistics.Admin.Security.AuthorizationHandlers;

public class ViewPublicApiDataSetsRequirement : IAuthorizationRequirement;

public class ViewPublicApiDataSetsAuthorizationHandler(IAuthorizationHandlerService authorizationHandlerService)
    : AuthorizationHandler<ViewPublicApiDataSetsRequirement, (User, Guid)>
{
    protected override async Task HandleRequirementAsync(
        AuthorizationHandlerContext context,
        ViewPublicApiDataSetsRequirement requirement,
        (User, Guid) userAndPublicationId
    )
    {
        var (user, publicationId) = userAndPublicationId;

        if (!user.Active)
        {
            return;
        }

        if (user.IsBau())
        {
            context.Succeed(requirement);
            return;
        }

        if (
            await authorizationHandlerService.UserHasAnyRoleOnPublication(userId: user.Id, publicationId: publicationId)
        )
        {
            context.Succeed(requirement);
        }
    }
}

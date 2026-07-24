#nullable enable
using GovUk.Education.ExploreEducationStatistics.Admin.Extensions;
using GovUk.Education.ExploreEducationStatistics.Common.Services.Security;
using GovUk.Education.ExploreEducationStatistics.Content.Model;
using Microsoft.AspNetCore.Authorization;

namespace GovUk.Education.ExploreEducationStatistics.Admin.Security.AuthorizationHandlers;

public class AccessAnalystPagesRequirement : IAuthorizationRequirement { }

public class AccessAnalystPagesAuthorizationHandler(IAuthorizationHandlerService authorizationHandlerService)
    : AuthorizationHandler<AccessAnalystPagesRequirement, User>
{
    protected override async Task HandleRequirementAsync(
        AuthorizationHandlerContext context,
        AccessAnalystPagesRequirement requirement,
        User user
    )
    {
        if (user.IsBau())
        {
            context.Succeed(requirement);
            return;
        }

        if (await authorizationHandlerService.UserHasAnyPublicationRole(context.User.GetUserId()))
        {
            context.Succeed(requirement);
        }
    }
}

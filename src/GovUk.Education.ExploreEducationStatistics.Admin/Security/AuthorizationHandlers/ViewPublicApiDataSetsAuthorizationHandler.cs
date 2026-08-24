#nullable enable
using GovUk.Education.ExploreEducationStatistics.Common.Services.Security;
using Microsoft.AspNetCore.Authorization;
using static GovUk.Education.ExploreEducationStatistics.Admin.Models.GlobalRoles;

namespace GovUk.Education.ExploreEducationStatistics.Admin.Security.AuthorizationHandlers;

public class ViewPublicApiDataSetsRequirement : IAuthorizationRequirement;

public class ViewPublicApiDataSetsAuthorizationHandler(IAuthorizationHandlerService authorizationHandlerService)
    : AuthorizationHandler<ViewPublicApiDataSetsRequirement, Guid>
{
    protected override async Task HandleRequirementAsync(
        AuthorizationHandlerContext context,
        ViewPublicApiDataSetsRequirement requirement,
        Guid publicationId
    )
    {
        if (context.User.IsInRole(RoleNames.BauUser))
        {
            context.Succeed(requirement);
            return;
        }

        if (
            await authorizationHandlerService.UserHasAnyRoleOnPublication(
                userId: context.User.GetUserId(),
                publicationId: publicationId
            )
        )
        {
            context.Succeed(requirement);
        }
    }
}

#nullable enable
using GovUk.Education.ExploreEducationStatistics.Common.Services.Security;
using GovUk.Education.ExploreEducationStatistics.Content.Model;
using Microsoft.AspNetCore.Authorization;
using static GovUk.Education.ExploreEducationStatistics.Admin.Models.GlobalRoles;

namespace GovUk.Education.ExploreEducationStatistics.Admin.Security.AuthorizationHandlers;

public class ManagePublicApiDataSetPreviewTokensRequirement : IAuthorizationRequirement;

public class ManagePublicApiDataSetPreviewTokensAuthorizationHandler(
    IAuthorizationHandlerService authorizationHandlerService
) : AuthorizationHandler<ManagePublicApiDataSetPreviewTokensRequirement, Guid>
{
    protected override async Task HandleRequirementAsync(
        AuthorizationHandlerContext context,
        ManagePublicApiDataSetPreviewTokensRequirement requirement,
        Guid publicationId
    )
    {
        if (context.User.IsInRole(RoleNames.BauUser))
        {
            context.Succeed(requirement);
            return;
        }

        if (
            await authorizationHandlerService.UserHasAnyPublicationRoleOnPublication(
                userId: context.User.GetUserId(),
                publicationId: publicationId,
                rolesToInclude: [PublicationRole.Drafter, PublicationRole.Approver]
            )
        )
        {
            context.Succeed(requirement);
        }
    }
}

#nullable enable
using GovUk.Education.ExploreEducationStatistics.Admin.Models;
using GovUk.Education.ExploreEducationStatistics.Common.Extensions;
using GovUk.Education.ExploreEducationStatistics.Common.Services.Security;
using GovUk.Education.ExploreEducationStatistics.Content.Model;
using Microsoft.AspNetCore.Authorization;
using static GovUk.Education.ExploreEducationStatistics.Content.Model.ReleaseApprovalStatus;

namespace GovUk.Education.ExploreEducationStatistics.Admin.Security.AuthorizationHandlers;

public class DeleteSpecificReleaseRequirement : IAuthorizationRequirement { }

public class DeleteSpecificReleaseAuthorizationHandler(IAuthorizationHandlerService authorizationHandlerService)
    : AuthorizationHandler<DeleteSpecificReleaseRequirement, ReleaseVersion>
{
    protected override async Task HandleRequirementAsync(
        AuthorizationHandlerContext context,
        DeleteSpecificReleaseRequirement requirement,
        ReleaseVersion releaseVersion
    )
    {
        if (releaseVersion.ApprovalStatus != Draft)
        {
            return;
        }

        if (!releaseVersion.Amendment)
        {
            if (context.User.IsInRole(GlobalRoles.Role.BauUser.GetEnumLabel()))
            {
                context.Succeed(requirement);
            }

            return;
        }

        if (SecurityUtils.HasClaim(context.User, SecurityClaimTypes.DeleteAllReleaseAmendments))
        {
            context.Succeed(requirement);
            return;
        }

        if (
            await authorizationHandlerService.UserHasAnyPublicationRoleOnPublication(
                userId: context.User.GetUserId(),
                publicationId: releaseVersion.Release.PublicationId,
                rolesToInclude: [PublicationRole.Drafter, PublicationRole.Approver]
            )
        )
        {
            context.Succeed(requirement);
        }
    }
}

#nullable enable
using GovUk.Education.ExploreEducationStatistics.Admin.Models;
using GovUk.Education.ExploreEducationStatistics.Common.Extensions;
using GovUk.Education.ExploreEducationStatistics.Common.Services.Security;
using GovUk.Education.ExploreEducationStatistics.Content.Model;
using Microsoft.AspNetCore.Authorization;
using static GovUk.Education.ExploreEducationStatistics.Admin.Security.SecurityClaimTypes;
using static GovUk.Education.ExploreEducationStatistics.Content.Model.PublicationRole;
using static GovUk.Education.ExploreEducationStatistics.Content.Model.ReleaseApprovalStatus;

namespace GovUk.Education.ExploreEducationStatistics.Admin.Security.AuthorizationHandlers;

public class DeleteSpecificReleaseRequirement : IAuthorizationRequirement { }

public class DeleteSpecificReleaseAuthorizationHandler
    : AuthorizationHandler<DeleteSpecificReleaseRequirement, ReleaseVersion>
{
    private readonly AuthorizationHandlerService _authorizationHandlerService;

    public DeleteSpecificReleaseAuthorizationHandler(AuthorizationHandlerService authorizationHandlerService)
    {
        _authorizationHandlerService = authorizationHandlerService;
    }

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
        }
        else
        {
            if (SecurityUtils.HasClaim(context.User, DeleteAllReleaseAmendments))
            {
                context.Succeed(requirement);
                return;
            }

            if (
                await _authorizationHandlerService.UserHasAnyPublicationRoleOnPublication(
                    userId: context.User.GetUserId(),
                    publicationId: releaseVersion.PublicationId,
                    Owner
                )
            )
            {
                context.Succeed(requirement);
            }
        }
    }
}

#nullable enable
using GovUk.Education.ExploreEducationStatistics.Admin.Services.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Common.Services.Security;
using GovUk.Education.ExploreEducationStatistics.Content.Model;
using GovUk.Education.ExploreEducationStatistics.Publisher.Model;
using Microsoft.AspNetCore.Authorization;
using static GovUk.Education.ExploreEducationStatistics.Common.Services.CollectionUtils;
using static GovUk.Education.ExploreEducationStatistics.Content.Model.ReleaseApprovalStatus;

namespace GovUk.Education.ExploreEducationStatistics.Admin.Security.AuthorizationHandlers;

public abstract class ReleaseStatusAuthorizationHandler<TRequirement>(
    IReleasePublishingStatusRepository releasePublishingStatusRepository,
    IAuthorizationHandlerService authorizationHandlerService
) : AuthorizationHandler<TRequirement, ReleaseVersion>
    where TRequirement : IAuthorizationRequirement
{
    protected abstract ReleaseApprovalStatus TargetApprovalStatus { get; }

    protected override async Task HandleRequirementAsync(
        AuthorizationHandlerContext context,
        TRequirement requirement,
        ReleaseVersion releaseVersion
    )
    {
        var statuses = await releasePublishingStatusRepository.GetAllByOverallStage(
            releaseVersion.Id,
            ReleasePublishingStatusOverallStage.Started,
            ReleasePublishingStatusOverallStage.Complete
        );

        if (statuses.Any() || releaseVersion.Published != null)
        {
            return;
        }

        switch (TargetApprovalStatus)
        {
            case Approved:
                await HandleMovingToApproved(context, requirement, releaseVersion);
                break;
            case HigherLevelReview:
                await HandleMovingToHigherLevelReview(context, requirement, releaseVersion);
                break;
            case Draft:
                await HandleMovingToDraft(context, requirement, releaseVersion);
                break;
            default:
                throw new ArgumentOutOfRangeException();
        }
    }

    private async Task HandleMovingToApproved(
        AuthorizationHandlerContext context,
        TRequirement requirement,
        ReleaseVersion releaseVersion
    )
    {
        if (SecurityUtils.HasClaim(context.User, SecurityClaimTypes.ApproveAllReleases))
        {
            context.Succeed(requirement);
            return;
        }

        if (
            await authorizationHandlerService.UserHasAnyPublicationRoleOnPublication(
                userId: context.User.GetUserId(),
                publicationId: releaseVersion.Release.PublicationId,
                rolesToInclude: [PublicationRole.Approver]
            )
        )
        {
            context.Succeed(requirement);
        }
    }

    private async Task HandleMovingToHigherLevelReview(
        AuthorizationHandlerContext context,
        TRequirement requirement,
        ReleaseVersion releaseVersion
    )
    {
        if (SecurityUtils.HasClaim(context.User, SecurityClaimTypes.SubmitAllReleasesToHigherReview))
        {
            context.Succeed(requirement);
            return;
        }

        var allowedPublicationRoles =
            releaseVersion.ApprovalStatus == Approved
                ? SetOf(PublicationRole.Approver)
                : SetOf(PublicationRole.Drafter, PublicationRole.Approver);

        if (
            await authorizationHandlerService.UserHasAnyPublicationRoleOnPublication(
                userId: context.User.GetUserId(),
                publicationId: releaseVersion.Release.PublicationId,
                rolesToInclude: allowedPublicationRoles
            )
        )
        {
            context.Succeed(requirement);
        }
    }

    private async Task HandleMovingToDraft(
        AuthorizationHandlerContext context,
        TRequirement requirement,
        ReleaseVersion releaseVersion
    )
    {
        if (SecurityUtils.HasClaim(context.User, SecurityClaimTypes.MarkAllReleasesAsDraft))
        {
            context.Succeed(requirement);
            return;
        }

        var allowedPublicationRoles =
            releaseVersion.ApprovalStatus == Approved
                ? SetOf(PublicationRole.Approver)
                : SetOf(PublicationRole.Drafter, PublicationRole.Approver);

        if (
            await authorizationHandlerService.UserHasAnyPublicationRoleOnPublication(
                userId: context.User.GetUserId(),
                publicationId: releaseVersion.Release.PublicationId,
                rolesToInclude: allowedPublicationRoles
            )
        )
        {
            context.Succeed(requirement);
        }
    }
}

public class MarkReleaseAsDraftRequirement : IAuthorizationRequirement { }

public class MarkReleaseAsDraftAuthorizationHandler(
    IReleasePublishingStatusRepository releasePublishingStatusRepository,
    IAuthorizationHandlerService authorizationHandlerService
)
    : ReleaseStatusAuthorizationHandler<MarkReleaseAsDraftRequirement>(
        releasePublishingStatusRepository,
        authorizationHandlerService
    )
{
    protected override ReleaseApprovalStatus TargetApprovalStatus => Draft;
}

public class MarkReleaseAsHigherLevelReviewRequirement : IAuthorizationRequirement { }

public class MarkReleaseAsHigherLevelReviewAuthorizationHandler(
    IReleasePublishingStatusRepository releasePublishingStatusRepository,
    IAuthorizationHandlerService authorizationHandlerService
)
    : ReleaseStatusAuthorizationHandler<MarkReleaseAsHigherLevelReviewRequirement>(
        releasePublishingStatusRepository,
        authorizationHandlerService
    )
{
    protected override ReleaseApprovalStatus TargetApprovalStatus => HigherLevelReview;
}

public class MarkReleaseAsApprovedRequirement : IAuthorizationRequirement { }

public class MarkReleaseAsApprovedAuthorizationHandler(
    IReleasePublishingStatusRepository releasePublishingStatusRepository,
    IAuthorizationHandlerService authorizationHandlerService
)
    : ReleaseStatusAuthorizationHandler<MarkReleaseAsApprovedRequirement>(
        releasePublishingStatusRepository,
        authorizationHandlerService
    )
{
    protected override ReleaseApprovalStatus TargetApprovalStatus => Approved;
}

#nullable enable
using GovUk.Education.ExploreEducationStatistics.Admin.Services.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Common.Services.Security;
using GovUk.Education.ExploreEducationStatistics.Content.Model;
using GovUk.Education.ExploreEducationStatistics.Publisher.Model;
using Microsoft.AspNetCore.Authorization;
using static
    GovUk.Education.ExploreEducationStatistics.Admin.Security.AuthorizationHandlers.AuthorizationHandlerService;
using static GovUk.Education.ExploreEducationStatistics.Common.Services.CollectionUtils;
using static GovUk.Education.ExploreEducationStatistics.Content.Model.ReleaseApprovalStatus;

namespace GovUk.Education.ExploreEducationStatistics.Admin.Security.AuthorizationHandlers;

public abstract class
    ReleaseStatusAuthorizationHandler<TRequirement> : AuthorizationHandler<TRequirement, ReleaseVersion>
    where TRequirement : IAuthorizationRequirement
{
    private readonly IReleasePublishingStatusRepository _releasePublishingStatusRepository;
    private readonly AuthorizationHandlerService _authorizationHandlerService;

    protected ReleaseStatusAuthorizationHandler(
        IReleasePublishingStatusRepository releasePublishingStatusRepository,
        AuthorizationHandlerService authorizationHandlerService)
    {
        _releasePublishingStatusRepository = releasePublishingStatusRepository;
        _authorizationHandlerService = authorizationHandlerService;
    }

    protected abstract ReleaseApprovalStatus TargetApprovalStatus { get; }

    protected override async Task HandleRequirementAsync(
        AuthorizationHandlerContext context,
        TRequirement requirement,
        ReleaseVersion releaseVersion)
    {
        var statuses = await _releasePublishingStatusRepository
            .GetAllByOverallStage(
                releaseVersion.Id,
                ReleasePublishingStatusOverallStage.Started,
                ReleasePublishingStatusOverallStage.Complete);

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
        ReleaseVersion releaseVersion)
    {
        if (SecurityUtils.HasClaim(context.User, SecurityClaimTypes.ApproveAllReleases))
        {
            context.Succeed(requirement);
            return;
        }

        if (await _authorizationHandlerService
                .HasRolesOnPublicationOrReleaseVersion(
                    userId: context.User.GetUserId(),
                    publicationId: releaseVersion.PublicationId,
                    releaseVersionId: releaseVersion.Id,
                    ListOf(PublicationRole.Allower),
                    ListOf(ReleaseRole.Approver)))
        {
            context.Succeed(requirement);
        }
    }

    private async Task HandleMovingToHigherLevelReview(
        AuthorizationHandlerContext context,
        TRequirement requirement,
        ReleaseVersion releaseVersion)
    {
        if (SecurityUtils.HasClaim(context.User, SecurityClaimTypes.SubmitAllReleasesToHigherReview))
        {
            context.Succeed(requirement);
            return;
        }

        var allowedPublicationRoles = releaseVersion.ApprovalStatus == Approved
            ? ListOf(PublicationRole.Allower)
            : ListOf(PublicationRole.Owner, PublicationRole.Allower);

        var allowedReleaseRoles = releaseVersion.ApprovalStatus == Approved
            ? ListOf(ReleaseRole.Approver)
            : ReleaseEditorAndApproverRoles;

        if (await _authorizationHandlerService
                .HasRolesOnPublicationOrReleaseVersion(
                    userId: context.User.GetUserId(),
                    publicationId: releaseVersion.PublicationId,
                    releaseVersionId: releaseVersion.Id,
                    allowedPublicationRoles,
                    allowedReleaseRoles))
        {
            context.Succeed(requirement);
        }
    }

    private async Task HandleMovingToDraft(
        AuthorizationHandlerContext context,
        TRequirement requirement,
        ReleaseVersion releaseVersion)
    {
        if (SecurityUtils.HasClaim(context.User, SecurityClaimTypes.MarkAllReleasesAsDraft))
        {
            context.Succeed(requirement);
            return;
        }

        var allowedPublicationRoles = releaseVersion.ApprovalStatus == Approved
            ? ListOf(PublicationRole.Allower)
            : ListOf(PublicationRole.Owner, PublicationRole.Allower);

        var allowedReleaseRoles = releaseVersion.ApprovalStatus == Approved
            ? ListOf(ReleaseRole.Approver)
            : ReleaseEditorAndApproverRoles;

        if (await _authorizationHandlerService
                .HasRolesOnPublicationOrReleaseVersion(
                    userId: context.User.GetUserId(),
                    publicationId: releaseVersion.PublicationId,
                    releaseVersionId: releaseVersion.Id,
                    allowedPublicationRoles,
                    allowedReleaseRoles))
        {
            context.Succeed(requirement);
        }
    }
}

public class MarkReleaseAsDraftRequirement : IAuthorizationRequirement
{
}

public class MarkReleaseAsDraftAuthorizationHandler
    : ReleaseStatusAuthorizationHandler<MarkReleaseAsDraftRequirement>
{
    public MarkReleaseAsDraftAuthorizationHandler(
        IReleasePublishingStatusRepository releasePublishingStatusRepository,
        AuthorizationHandlerService authorizationHandlerService)
        : base(
            releasePublishingStatusRepository,
            authorizationHandlerService)
    {
    }

    protected override ReleaseApprovalStatus TargetApprovalStatus => Draft;
}

public class MarkReleaseAsHigherLevelReviewRequirement : IAuthorizationRequirement
{
}

public class MarkReleaseAsHigherLevelReviewAuthorizationHandler
    : ReleaseStatusAuthorizationHandler<MarkReleaseAsHigherLevelReviewRequirement>
{
    public MarkReleaseAsHigherLevelReviewAuthorizationHandler(
        IReleasePublishingStatusRepository releasePublishingStatusRepository,
        AuthorizationHandlerService authorizationHandlerService)
        : base(
            releasePublishingStatusRepository,
            authorizationHandlerService)
    {
    }

    protected override ReleaseApprovalStatus TargetApprovalStatus => HigherLevelReview;
}

public class MarkReleaseAsApprovedRequirement : IAuthorizationRequirement
{
}

public class MarkReleaseAsApprovedAuthorizationHandler
    : ReleaseStatusAuthorizationHandler<MarkReleaseAsApprovedRequirement>
{
    public MarkReleaseAsApprovedAuthorizationHandler(
        IReleasePublishingStatusRepository releasePublishingStatusRepository,
        AuthorizationHandlerService authorizationHandlerService)
        : base(
            releasePublishingStatusRepository,
            authorizationHandlerService)
    {
    }

    protected override ReleaseApprovalStatus TargetApprovalStatus => Approved;
}

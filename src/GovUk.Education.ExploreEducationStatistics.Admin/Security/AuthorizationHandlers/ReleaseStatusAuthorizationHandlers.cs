using System;
using System.Linq;
using System.Threading.Tasks;
using GovUk.Education.ExploreEducationStatistics.Admin.Services.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Common.Services.Security;
using GovUk.Education.ExploreEducationStatistics.Content.Model;
using GovUk.Education.ExploreEducationStatistics.Publisher.Model;
using Microsoft.AspNetCore.Authorization;
using static GovUk.Education.ExploreEducationStatistics.Admin.Security.AuthorizationHandlers.AuthorizationHandlerService;
using static GovUk.Education.ExploreEducationStatistics.Common.Services.CollectionUtils;
using static GovUk.Education.ExploreEducationStatistics.Content.Model.ReleaseApprovalStatus;

namespace GovUk.Education.ExploreEducationStatistics.Admin.Security.AuthorizationHandlers
{
    public abstract class ReleaseStatusAuthorizationHandler<TRequirement> : AuthorizationHandler<TRequirement, Release>
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
            Release release)
        {
            var statuses = await _releasePublishingStatusRepository
                .GetAllByOverallStage(
                    release.Id,
                    ReleasePublishingStatusOverallStage.Started,
                    ReleasePublishingStatusOverallStage.Complete);

            if (statuses.Any() || release.Published != null)
            {
                return;
            }

            switch (TargetApprovalStatus)
            {
                case Approved:
                    await HandleMovingToApproved(context, requirement, release);
                    break;
                case HigherLevelReview:
                    await HandleMovingToHigherLevelReview(context, requirement, release);
                    break;
                case Draft:
                    await HandleMovingToDraft(context, requirement, release);
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        private async Task HandleMovingToApproved(
            AuthorizationHandlerContext context,
            TRequirement requirement,
            Release release)
        {
            if (SecurityUtils.HasClaim(context.User, SecurityClaimTypes.ApproveAllReleases))
            {
                context.Succeed(requirement);
                return;
            }
            
            if (await _authorizationHandlerService
                    .HasRolesOnPublicationOrRelease(
                        context.User.GetUserId(),
                        release.PublicationId,
                        release.Id,
                        ListOf(PublicationRole.Approver),
                        ListOf(ReleaseRole.Approver)))
            {
                context.Succeed(requirement);
            }
        }

        private async Task HandleMovingToHigherLevelReview(
            AuthorizationHandlerContext context,
            TRequirement requirement,
            Release release)
        {
            if (SecurityUtils.HasClaim(context.User, SecurityClaimTypes.SubmitAllReleasesToHigherReview))
            {
                context.Succeed(requirement);
                return;
            }

            var allowedPublicationRoles = release.ApprovalStatus == Approved
                ? ListOf(PublicationRole.Approver)
                : ListOf(PublicationRole.Owner, PublicationRole.Approver);
            
            var allowedReleaseRoles = release.ApprovalStatus == Approved
                ? ListOf(ReleaseRole.Approver)
                : ReleaseEditorAndApproverRoles;

            if (await _authorizationHandlerService
                    .HasRolesOnPublicationOrRelease(
                        context.User.GetUserId(),
                        release.PublicationId,
                        release.Id,
                        allowedPublicationRoles,
                        allowedReleaseRoles))
            {
                context.Succeed(requirement);
            }
        }

        private async Task HandleMovingToDraft(
            AuthorizationHandlerContext context,
            TRequirement requirement,
            Release release)
        {
            if (SecurityUtils.HasClaim(context.User, SecurityClaimTypes.MarkAllReleasesAsDraft))
            {
                context.Succeed(requirement);
                return;
            }

            var allowedPublicationRoles = release.ApprovalStatus == Approved
                ? ListOf(PublicationRole.Approver)
                : ListOf(PublicationRole.Owner, PublicationRole.Approver);
            
            var allowedReleaseRoles = release.ApprovalStatus == Approved
                ? ListOf(ReleaseRole.Approver)
                : ReleaseEditorAndApproverRoles;

            if (await _authorizationHandlerService
                    .HasRolesOnPublicationOrRelease(
                        context.User.GetUserId(),
                        release.PublicationId,
                        release.Id,
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
}

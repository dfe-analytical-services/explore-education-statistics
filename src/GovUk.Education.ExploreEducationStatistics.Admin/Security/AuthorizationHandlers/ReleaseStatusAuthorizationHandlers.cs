using System;
using System.Linq;
using System.Threading.Tasks;
using GovUk.Education.ExploreEducationStatistics.Admin.Services.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Common.Services.Security;
using GovUk.Education.ExploreEducationStatistics.Content.Model;
using GovUk.Education.ExploreEducationStatistics.Publisher.Model;
using Microsoft.AspNetCore.Authorization;
using static GovUk.Education.ExploreEducationStatistics.Admin.Security.AuthorizationHandlers.AuthorizationHandlerUtil;

namespace GovUk.Education.ExploreEducationStatistics.Admin.Security.AuthorizationHandlers
{
    public abstract class ReleaseStatusAuthorizationHandler<TRequirement> : AuthorizationHandler<TRequirement, Release>
        where TRequirement : IAuthorizationRequirement
    {
        private readonly IReleasePublishingStatusRepository _releasePublishingStatusRepository;
        private readonly IUserPublicationRoleRepository _userPublicationRoleRepository;
        private readonly IUserReleaseRoleRepository _userReleaseRoleRepository;

        protected ReleaseStatusAuthorizationHandler(IReleasePublishingStatusRepository releasePublishingStatusRepository,
            IUserPublicationRoleRepository userPublicationRoleRepository,
            IUserReleaseRoleRepository userReleaseRoleRepository)
        {
            _releasePublishingStatusRepository = releasePublishingStatusRepository;
            _userPublicationRoleRepository = userPublicationRoleRepository;
            _userReleaseRoleRepository = userReleaseRoleRepository;
        }

        protected abstract ReleaseApprovalStatus TargetApprovalStatus { get; }

        protected override async Task HandleRequirementAsync(
            AuthorizationHandlerContext context,
            TRequirement requirement,
            Release release)
        {
            var statuses = await _releasePublishingStatusRepository.GetAllByOverallStage(
                release.Id,
                ReleasePublishingStatusOverallStage.Started,
                ReleasePublishingStatusOverallStage.Complete
            );

            if (statuses.Any() || release.Published != null)
            {
                return;
            }

            switch (TargetApprovalStatus)
            {
                case ReleaseApprovalStatus.Approved:
                    await HandleApproved(context, requirement, release);
                    break;
                case ReleaseApprovalStatus.HigherLevelReview:
                    await HandleHigherLevelReview(context, requirement, release);
                    break;
                case ReleaseApprovalStatus.Draft:
                    await HandleDraft(context, requirement, release);
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        private async Task HandleApproved(
            AuthorizationHandlerContext context,
            TRequirement requirement,
            Release release)
        {
            if (SecurityUtils.HasClaim(context.User, SecurityClaimTypes.ApproveAllReleases))
            {
                context.Succeed(requirement);
                return;
            }

            var roles = await _userReleaseRoleRepository.GetAllRolesByUserAndRelease(context.User.GetUserId(), release.Id);

            if (ContainsApproverRole(roles))
            {
                context.Succeed(requirement);
            }
        }

        private async Task HandleHigherLevelReview(
            AuthorizationHandlerContext context,
            TRequirement requirement,
            Release release)
        {
            if (SecurityUtils.HasClaim(context.User, SecurityClaimTypes.SubmitAllReleasesToHigherReview))
            {
                context.Succeed(requirement);
                return;
            }

            var publicationRoles =
                await _userPublicationRoleRepository.GetAllRolesByUserAndPublicationId(context.User.GetUserId(), release.PublicationId);
            var releaseRoles = await _userReleaseRoleRepository.GetAllRolesByUserAndRelease(context.User.GetUserId(), release.Id);

            if (release.ApprovalStatus == ReleaseApprovalStatus.Approved
                ? ContainsApproverRole(releaseRoles)
                : ContainPublicationOwnerRole(publicationRoles) || ContainsEditorOrApproverRole(releaseRoles))
            {
                context.Succeed(requirement);
            }
        }

        private async Task HandleDraft(
            AuthorizationHandlerContext context,
            TRequirement requirement,
            Release release)
        {
            if (SecurityUtils.HasClaim(context.User, SecurityClaimTypes.MarkAllReleasesAsDraft))
            {
                context.Succeed(requirement);
                return;
            }

            var publicationRoles =
                await _userPublicationRoleRepository.GetAllRolesByUserAndPublicationId(context.User.GetUserId(), release.PublicationId);
            var releaseRoles = await _userReleaseRoleRepository.GetAllRolesByUserAndRelease(context.User.GetUserId(), release.Id);

            if (release.ApprovalStatus == ReleaseApprovalStatus.Approved
                ? ContainsApproverRole(releaseRoles)
                : ContainPublicationOwnerRole(publicationRoles) || ContainsEditorOrApproverRole(releaseRoles))
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
        public MarkReleaseAsDraftAuthorizationHandler(IReleasePublishingStatusRepository releasePublishingStatusRepository,
            IUserPublicationRoleRepository userPublicationRoleRepository,
            IUserReleaseRoleRepository userReleaseRoleRepository) : base(releasePublishingStatusRepository,
            userPublicationRoleRepository,
            userReleaseRoleRepository)
        {
        }

        protected override ReleaseApprovalStatus TargetApprovalStatus => ReleaseApprovalStatus.Draft;
    }

    public class MarkReleaseAsHigherLevelReviewRequirement : IAuthorizationRequirement
    {
    }

    public class MarkReleaseAsHigherLevelReviewAuthorizationHandler
        : ReleaseStatusAuthorizationHandler<MarkReleaseAsHigherLevelReviewRequirement>
    {
        public MarkReleaseAsHigherLevelReviewAuthorizationHandler(IReleasePublishingStatusRepository releasePublishingStatusRepository,
            IUserPublicationRoleRepository userPublicationRoleRepository,
            IUserReleaseRoleRepository userReleaseRoleRepository) : base(releasePublishingStatusRepository,
            userPublicationRoleRepository,
            userReleaseRoleRepository)
        {
        }

        protected override ReleaseApprovalStatus TargetApprovalStatus => ReleaseApprovalStatus.HigherLevelReview;
    }

    public class MarkReleaseAsApprovedRequirement : IAuthorizationRequirement
    {
    }

    public class MarkReleaseAsApprovedAuthorizationHandler
        : ReleaseStatusAuthorizationHandler<MarkReleaseAsApprovedRequirement>
    {
        public MarkReleaseAsApprovedAuthorizationHandler(
            IReleasePublishingStatusRepository releasePublishingStatusRepository,
            IUserPublicationRoleRepository userPublicationRoleRepository,
            IUserReleaseRoleRepository userReleaseRoleRepository) : base(releasePublishingStatusRepository,
            userPublicationRoleRepository,
            userReleaseRoleRepository)
        {
        }

        protected override ReleaseApprovalStatus TargetApprovalStatus => ReleaseApprovalStatus.Approved;
    }
}

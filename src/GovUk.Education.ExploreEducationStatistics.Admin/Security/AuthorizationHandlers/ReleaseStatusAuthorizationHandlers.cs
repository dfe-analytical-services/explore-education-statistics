using System;
using System.Linq;
using System.Threading.Tasks;
using GovUk.Education.ExploreEducationStatistics.Admin.Services.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Common.Services.Security;
using GovUk.Education.ExploreEducationStatistics.Content.Model;
using GovUk.Education.ExploreEducationStatistics.Publisher.Model;
using Microsoft.AspNetCore.Authorization;
using static GovUk.Education.ExploreEducationStatistics.Admin.Security.AuthorizationHandlers.AuthorizationHandlerUtil;
using ReleaseStatus = GovUk.Education.ExploreEducationStatistics.Content.Model.ReleaseStatus;

namespace GovUk.Education.ExploreEducationStatistics.Admin.Security.AuthorizationHandlers
{
    public abstract class ReleaseStatusAuthorizationHandler<TRequirement> : AuthorizationHandler<TRequirement, Release>
        where TRequirement : IAuthorizationRequirement
    {
        private readonly IReleaseStatusRepository _releaseStatusRepository;
        private readonly IUserPublicationRoleRepository _userPublicationRoleRepository;
        private readonly IUserReleaseRoleRepository _userReleaseRoleRepository;

        protected ReleaseStatusAuthorizationHandler(IReleaseStatusRepository releaseStatusRepository,
            IUserPublicationRoleRepository userPublicationRoleRepository,
            IUserReleaseRoleRepository userReleaseRoleRepository)
        {
            _releaseStatusRepository = releaseStatusRepository;
            _userPublicationRoleRepository = userPublicationRoleRepository;
            _userReleaseRoleRepository = userReleaseRoleRepository;
        }

        protected abstract ReleaseStatus TargetStatus { get; }

        protected override async Task HandleRequirementAsync(
            AuthorizationHandlerContext context,
            TRequirement requirement,
            Release release)
        {
            var statuses = await _releaseStatusRepository.GetAllByOverallStage(
                release.Id,
                ReleaseStatusOverallStage.Started,
                ReleaseStatusOverallStage.Complete
            );

            if (statuses.Any() || release.Published != null)
            {
                return;
            }

            switch (TargetStatus)
            {
                case ReleaseStatus.Approved:
                    await HandleApproved(context, requirement, release);
                    break;
                case ReleaseStatus.HigherLevelReview:
                    await HandleHigherLevelReview(context, requirement, release);
                    break;
                case ReleaseStatus.Draft:
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

            var roles = await _userReleaseRoleRepository.GetAllRolesByUser(context.User.GetUserId(), release.Id);

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
                await _userPublicationRoleRepository.GetAllRolesByUser(context.User.GetUserId(), release.PublicationId);
            var releaseRoles = await _userReleaseRoleRepository.GetAllRolesByUser(context.User.GetUserId(), release.Id);

            if (release.Status == ReleaseStatus.Approved
                ? ContainsApproverRole(releaseRoles)
                : ContainPublicationOwnerRole(publicationRoles) || ContainsEditorRole(releaseRoles))
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
                await _userPublicationRoleRepository.GetAllRolesByUser(context.User.GetUserId(), release.PublicationId);
            var releaseRoles = await _userReleaseRoleRepository.GetAllRolesByUser(context.User.GetUserId(), release.Id);

            if (release.Status == ReleaseStatus.Approved
                ? ContainsApproverRole(releaseRoles)
                : ContainPublicationOwnerRole(publicationRoles) || ContainsEditorRole(releaseRoles))
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
        public MarkReleaseAsDraftAuthorizationHandler(IReleaseStatusRepository releaseStatusRepository,
            IUserPublicationRoleRepository userPublicationRoleRepository,
            IUserReleaseRoleRepository userReleaseRoleRepository) : base(releaseStatusRepository,
            userPublicationRoleRepository,
            userReleaseRoleRepository)
        {
        }

        protected override ReleaseStatus TargetStatus => ReleaseStatus.Draft;
    }

    public class MarkReleaseAsHigherLevelReviewRequirement : IAuthorizationRequirement
    {
    }

    public class MarkReleaseAsHigherLevelReviewAuthorizationHandler
        : ReleaseStatusAuthorizationHandler<MarkReleaseAsHigherLevelReviewRequirement>
    {
        public MarkReleaseAsHigherLevelReviewAuthorizationHandler(IReleaseStatusRepository releaseStatusRepository,
            IUserPublicationRoleRepository userPublicationRoleRepository,
            IUserReleaseRoleRepository userReleaseRoleRepository) : base(releaseStatusRepository,
            userPublicationRoleRepository,
            userReleaseRoleRepository)
        {
        }

        protected override ReleaseStatus TargetStatus => ReleaseStatus.HigherLevelReview;
    }

    public class MarkReleaseAsApprovedRequirement : IAuthorizationRequirement
    {
    }

    public class MarkReleaseAsApprovedAuthorizationHandler
        : ReleaseStatusAuthorizationHandler<MarkReleaseAsApprovedRequirement>
    {
        public MarkReleaseAsApprovedAuthorizationHandler(
            IReleaseStatusRepository releaseStatusRepository,
            IUserPublicationRoleRepository userPublicationRoleRepository,
            IUserReleaseRoleRepository userReleaseRoleRepository) : base(releaseStatusRepository,
            userPublicationRoleRepository,
            userReleaseRoleRepository)
        {
        }

        protected override ReleaseStatus TargetStatus => ReleaseStatus.Approved;
    }
}

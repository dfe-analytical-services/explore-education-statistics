using System;
using System.Linq;
using System.Threading.Tasks;
using GovUk.Education.ExploreEducationStatistics.Admin.Services.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Content.Model;
using GovUk.Education.ExploreEducationStatistics.Content.Model.Database;
using GovUk.Education.ExploreEducationStatistics.Publisher.Model;
using Microsoft.AspNetCore.Authorization;
using static GovUk.Education.ExploreEducationStatistics.Admin.Security.AuthorizationHandlers.AuthorizationHandlerUtil;
using ReleaseStatus = GovUk.Education.ExploreEducationStatistics.Content.Model.ReleaseStatus;

namespace GovUk.Education.ExploreEducationStatistics.Admin.Security.AuthorizationHandlers
{
    public abstract class ReleaseStatusAuthorizationHandler<TRequirement> : AuthorizationHandler<TRequirement, Release>
        where TRequirement : IAuthorizationRequirement
    {
        private readonly ContentDbContext _context;
        private readonly IReleaseStatusRepository _releaseStatusRepository;

        public ReleaseStatusAuthorizationHandler(
            ContentDbContext context,
            IReleaseStatusRepository releaseStatusRepository)
        {
            _context = context;
            _releaseStatusRepository = releaseStatusRepository;
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
                    HandleApproved(context, requirement, release);
                    break;
                case ReleaseStatus.HigherLevelReview:
                    HandleHigherLevelReview(context, requirement, release);
                    break;
                case ReleaseStatus.Draft:
                    HandleDraft(context, requirement, release);
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        private void HandleApproved(
            AuthorizationHandlerContext context,
            TRequirement requirement,
            Release release)
        {
            if (SecurityUtils.HasClaim(context.User, SecurityClaimTypes.ApproveAllReleases))
            {
                context.Succeed(requirement);
                return;
            }

            var roles = GetReleaseRoles(context.User, release, _context);

            if (ContainsApproverRole(roles))
            {
                context.Succeed(requirement);
            }
        }

        private void HandleHigherLevelReview(
            AuthorizationHandlerContext context,
            TRequirement requirement,
            Release release)
        {
            if (SecurityUtils.HasClaim(context.User, SecurityClaimTypes.SubmitAllReleasesToHigherReview))
            {
                context.Succeed(requirement);
                return;
            }

            var roles = GetReleaseRoles(context.User, release, _context);

            if (release.Status == ReleaseStatus.Approved
                ? ContainsApproverRole(roles)
                : ContainsEditorRole(roles))
            {
                context.Succeed(requirement);
            }
        }

        private void HandleDraft(
            AuthorizationHandlerContext context,
            TRequirement requirement,
            Release release)
        {
            if (SecurityUtils.HasClaim(context.User, SecurityClaimTypes.MarkAllReleasesAsDraft))
            {
                context.Succeed(requirement);
                return;
            }

            var roles = GetReleaseRoles(context.User, release, _context);

            if (release.Status == ReleaseStatus.Approved
                ? ContainsApproverRole(roles)
                : ContainsEditorRole(roles))
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
            ContentDbContext context,
            IReleaseStatusRepository releaseStatusRepository) : base(context, releaseStatusRepository)
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
        public MarkReleaseAsHigherLevelReviewAuthorizationHandler(
            ContentDbContext context,
            IReleaseStatusRepository releaseStatusRepository) : base(context, releaseStatusRepository)
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
            ContentDbContext context,
            IReleaseStatusRepository releaseStatusRepository) : base(context, releaseStatusRepository)
        {
        }

        protected override ReleaseStatus TargetStatus => ReleaseStatus.Approved;
    }
}
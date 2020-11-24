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
    public class UpdateSpecificReleaseRequirement : IAuthorizationRequirement
    {
    }

    public class UpdateSpecificReleaseAuthorizationHandler
        : AuthorizationHandler<UpdateSpecificReleaseRequirement, Release>
    {
        private readonly ContentDbContext _context;
        private readonly IReleaseStatusRepository _releaseStatusRepository;

        public UpdateSpecificReleaseAuthorizationHandler(
            ContentDbContext context,
            IReleaseStatusRepository releaseStatusRepository)
        {
            _context = context;
            _releaseStatusRepository = releaseStatusRepository;
        }

        protected override async Task HandleRequirementAsync(
            AuthorizationHandlerContext context,
            UpdateSpecificReleaseRequirement requirement,
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

            if (SecurityUtils.HasClaim(context.User, SecurityClaimTypes.UpdateAllReleases))
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
}
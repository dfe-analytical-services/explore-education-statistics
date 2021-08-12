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
    public class UpdateSpecificReleaseRequirement : IAuthorizationRequirement
    {
    }

    public class UpdateSpecificReleaseAuthorizationHandler
        : AuthorizationHandler<UpdateSpecificReleaseRequirement, Release>
    {
        private readonly IReleaseStatusRepository _releaseStatusRepository;
        private readonly IUserPublicationRoleRepository _publicationRoleRepository;
        private readonly IUserReleaseRoleRepository _releaseRoleRepository;

        public UpdateSpecificReleaseAuthorizationHandler(IReleaseStatusRepository releaseStatusRepository,
            IUserPublicationRoleRepository publicationRoleRepository,
            IUserReleaseRoleRepository releaseRoleRepository)
        {
            _releaseStatusRepository = releaseStatusRepository;
            _publicationRoleRepository = publicationRoleRepository;
            _releaseRoleRepository = releaseRoleRepository;
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

            var publicationRoles =
                await _publicationRoleRepository.GetAllRolesByUser(context.User.GetUserId(), release.PublicationId);
            var releaseRoles = await _releaseRoleRepository.GetAllRolesByUser(context.User.GetUserId(), release.Id);

            if (release.ApprovalStatus == ReleaseApprovalStatus.Approved
                ? ContainsApproverRole(releaseRoles)
                : ContainPublicationOwnerRole(publicationRoles) || ContainsEditorOrApproverRole(releaseRoles))
            {
                context.Succeed(requirement);
            }
        }
    }
}

using System.Linq;
using System.Threading.Tasks;
using GovUk.Education.ExploreEducationStatistics.Admin.Services.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Common.Services.Security;
using GovUk.Education.ExploreEducationStatistics.Content.Model;
using GovUk.Education.ExploreEducationStatistics.Publisher.Model;
using Microsoft.AspNetCore.Authorization;
using static GovUk.Education.ExploreEducationStatistics.Admin.Security.AuthorizationHandlers.AuthorizationHandlerResourceRoleService;
using static GovUk.Education.ExploreEducationStatistics.Common.Services.CollectionUtils;
using static GovUk.Education.ExploreEducationStatistics.Content.Model.PublicationRole;
using static GovUk.Education.ExploreEducationStatistics.Content.Model.ReleaseApprovalStatus;

namespace GovUk.Education.ExploreEducationStatistics.Admin.Security.AuthorizationHandlers
{
    public class UpdateSpecificReleaseRequirement : IAuthorizationRequirement
    {
    }

    public class UpdateSpecificReleaseAuthorizationHandler
        : AuthorizationHandler<UpdateSpecificReleaseRequirement, Release>
    {
        private readonly IReleasePublishingStatusRepository _releasePublishingStatusRepository;
        private readonly AuthorizationHandlerResourceRoleService _authorizationHandlerResourceRoleService;

        public UpdateSpecificReleaseAuthorizationHandler(
            IReleasePublishingStatusRepository releasePublishingStatusRepository,
            AuthorizationHandlerResourceRoleService authorizationHandlerResourceRoleService)
        {
            _releasePublishingStatusRepository = releasePublishingStatusRepository;
            _authorizationHandlerResourceRoleService = authorizationHandlerResourceRoleService;
        }

        protected override async Task HandleRequirementAsync(
            AuthorizationHandlerContext context,
            UpdateSpecificReleaseRequirement requirement,
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

            if (SecurityUtils.HasClaim(context.User, SecurityClaimTypes.UpdateAllReleases))
            {
                context.Succeed(requirement);
                return;
            }
            
            var allowedPublicationRoles = release.ApprovalStatus == Approved
                ? ListOf(Approver)
                : ListOf(Owner, Approver);
            
            var allowedReleaseRoles = release.ApprovalStatus == Approved
                ? ListOf(ReleaseRole.Approver)
                : ReleaseEditorAndApproverRoles;

            if (await _authorizationHandlerResourceRoleService
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
}

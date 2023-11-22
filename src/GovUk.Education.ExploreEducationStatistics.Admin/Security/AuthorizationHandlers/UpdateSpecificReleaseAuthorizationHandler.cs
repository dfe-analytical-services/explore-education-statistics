using System.Threading.Tasks;
using GovUk.Education.ExploreEducationStatistics.Common.Services.Security;
using GovUk.Education.ExploreEducationStatistics.Content.Model;
using Microsoft.AspNetCore.Authorization;
using static GovUk.Education.ExploreEducationStatistics.Admin.Security.AuthorizationHandlers.AuthorizationHandlerService;
using static GovUk.Education.ExploreEducationStatistics.Common.Services.CollectionUtils;
using static GovUk.Education.ExploreEducationStatistics.Content.Model.ReleaseApprovalStatus;

namespace GovUk.Education.ExploreEducationStatistics.Admin.Security.AuthorizationHandlers
{
    public class UpdateSpecificReleaseRequirement : IAuthorizationRequirement
    {
    }

    public class UpdateSpecificReleaseAuthorizationHandler
        : AuthorizationHandler<UpdateSpecificReleaseRequirement, Release>
    {
        private readonly AuthorizationHandlerService _authorizationHandlerService;

        public UpdateSpecificReleaseAuthorizationHandler(
            AuthorizationHandlerService authorizationHandlerService)
        {
            _authorizationHandlerService = authorizationHandlerService;
        }

        protected override async Task HandleRequirementAsync(
            AuthorizationHandlerContext context,
            UpdateSpecificReleaseRequirement requirement,
            Release release)
        {
            if (release.ApprovalStatus == Approved)
            {
                return;
            }

            if (SecurityUtils.HasClaim(context.User, SecurityClaimTypes.UpdateAllReleases))
            {
                context.Succeed(requirement);
                return;
            }

            var allowedPublicationRoles = ListOf(PublicationRole.Owner, PublicationRole.Approver);
            var allowedReleaseRoles = ReleaseEditorAndApproverRoles;

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
}

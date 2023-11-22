using System.Threading.Tasks;
using GovUk.Education.ExploreEducationStatistics.Common.Services.Security;
using GovUk.Education.ExploreEducationStatistics.Content.Model;
using Microsoft.AspNetCore.Authorization;
using static GovUk.Education.ExploreEducationStatistics.Admin.Security.AuthorizationHandlers.AuthorizationHandlerService;
using static GovUk.Education.ExploreEducationStatistics.Common.Services.CollectionUtils;

namespace GovUk.Education.ExploreEducationStatistics.Admin.Security.AuthorizationHandlers
{
    public class ViewReleaseStatusHistoryRequirement : IAuthorizationRequirement
    {
    }

    public class ViewReleaseStatusHistoryAuthorizationHandler
        : AuthorizationHandler<ViewReleaseStatusHistoryRequirement, Release>
    {
        private readonly AuthorizationHandlerService _authorizationHandlerService;

        public ViewReleaseStatusHistoryAuthorizationHandler(
            AuthorizationHandlerService authorizationHandlerService)
        {
            _authorizationHandlerService = authorizationHandlerService;
        }

        protected override async Task HandleRequirementAsync(
            AuthorizationHandlerContext context,
            ViewReleaseStatusHistoryRequirement requirement,
            Release release)
        {
            if (SecurityUtils.HasClaim(context.User, SecurityClaimTypes.AccessAllReleases))
            {
                context.Succeed(requirement);
                return;
            }
            
            if (await _authorizationHandlerService
                    .HasRolesOnPublicationOrRelease(
                        context.User.GetUserId(),
                        release.PublicationId,
                        release.Id,
                        ListOf(PublicationRole.Owner, PublicationRole.Approver),
                        UnrestrictedReleaseViewerRoles))
            {
                context.Succeed(requirement);
            }
        }
    }
}

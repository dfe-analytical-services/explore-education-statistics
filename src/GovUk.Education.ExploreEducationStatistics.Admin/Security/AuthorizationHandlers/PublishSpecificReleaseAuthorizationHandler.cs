using System.Threading.Tasks;
using GovUk.Education.ExploreEducationStatistics.Common.Services.Security;
using GovUk.Education.ExploreEducationStatistics.Content.Model;
using Microsoft.AspNetCore.Authorization;
using static GovUk.Education.ExploreEducationStatistics.Admin.Security.SecurityClaimTypes;
using static GovUk.Education.ExploreEducationStatistics.Common.Services.CollectionUtils;

namespace GovUk.Education.ExploreEducationStatistics.Admin.Security.AuthorizationHandlers
{
    public class PublishSpecificReleaseRequirement : IAuthorizationRequirement
    {
    }

    public class
        PublishSpecificReleaseAuthorizationHandler : AuthorizationHandler<PublishSpecificReleaseRequirement, Release>
    {
        private readonly AuthorizationHandlerService _authorizationHandlerService;

        public PublishSpecificReleaseAuthorizationHandler(
            AuthorizationHandlerService authorizationHandlerService)
        {
            _authorizationHandlerService = authorizationHandlerService;
        }

        protected override async Task HandleRequirementAsync(
            AuthorizationHandlerContext context,
            PublishSpecificReleaseRequirement requirement,
            Release release)
        {
            if (release.ApprovalStatus != ReleaseApprovalStatus.Approved)
            {
                return;
            }

            if (SecurityUtils.HasClaim(context.User, PublishAllReleases))
            {
                context.Succeed(requirement);
                return;
            }
            
            if (await _authorizationHandlerService
                    .HasRolesOnPublicationOrRelease(
                        context.User.GetUserId(),
                        release.PublicationId,
                        release.Id,
                        ListOf(PublicationRole.Owner),
                        ListOf(ReleaseRole.Approver)))
            {
                context.Succeed(requirement);
            }
        }
    }
}

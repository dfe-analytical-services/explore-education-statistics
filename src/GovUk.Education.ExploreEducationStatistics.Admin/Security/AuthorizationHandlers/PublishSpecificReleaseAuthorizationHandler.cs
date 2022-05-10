using System.Threading.Tasks;
using GovUk.Education.ExploreEducationStatistics.Admin.Services.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Common.Services.Security;
using GovUk.Education.ExploreEducationStatistics.Content.Model;
using Microsoft.AspNetCore.Authorization;
using static GovUk.Education.ExploreEducationStatistics.Admin.Security.AuthorizationHandlers.AuthorizationHandlerUtil;
using static GovUk.Education.ExploreEducationStatistics.Admin.Security.SecurityClaimTypes;

namespace GovUk.Education.ExploreEducationStatistics.Admin.Security.AuthorizationHandlers
{
    public class PublishSpecificReleaseRequirement : IAuthorizationRequirement
    {
    }

    public class
        PublishSpecificReleaseAuthorizationHandler : AuthorizationHandler<PublishSpecificReleaseRequirement, Release>
    {
        private readonly IUserReleaseRoleRepository _userReleaseRoleRepository;

        public PublishSpecificReleaseAuthorizationHandler(IUserReleaseRoleRepository userReleaseRoleRepository)
        {
            _userReleaseRoleRepository = userReleaseRoleRepository;
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

            var releaseRoles = await _userReleaseRoleRepository.GetAllRolesByUserAndRelease(context.User.GetUserId(), release.Id);

            if (ContainsApproverRole(releaseRoles))
            {
                context.Succeed(requirement);
            }
        }
    }
}

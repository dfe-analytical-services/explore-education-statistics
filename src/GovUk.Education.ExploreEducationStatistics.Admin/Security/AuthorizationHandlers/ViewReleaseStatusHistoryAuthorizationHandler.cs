using System.Threading.Tasks;
using GovUk.Education.ExploreEducationStatistics.Admin.Services.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Common.Services.Security;
using GovUk.Education.ExploreEducationStatistics.Content.Model;
using Microsoft.AspNetCore.Authorization;
using static GovUk.Education.ExploreEducationStatistics.Admin.Security.AuthorizationHandlers.AuthorizationHandlerUtil;

namespace GovUk.Education.ExploreEducationStatistics.Admin.Security.AuthorizationHandlers
{
    public class ViewReleaseStatusHistoryRequirement : IAuthorizationRequirement
    {
    }

    public class ViewReleaseStatusHistoryAuthorizationHandler
        : AuthorizationHandler<ViewReleaseStatusHistoryRequirement, Release>
    {
        private readonly IUserPublicationRoleRepository _publicationRoleRepository;
        private readonly IUserReleaseRoleRepository _releaseRoleRepository;

        public ViewReleaseStatusHistoryAuthorizationHandler(
            IUserPublicationRoleRepository publicationRoleRepository,
            IUserReleaseRoleRepository releaseRoleRepository)
        {
            _publicationRoleRepository = publicationRoleRepository;
            _releaseRoleRepository = releaseRoleRepository;
        }

        protected override async Task HandleRequirementAsync(
            AuthorizationHandlerContext authContext,
            ViewReleaseStatusHistoryRequirement requirement,
            Release release)
        {
            if (SecurityUtils.HasClaim(authContext.User, SecurityClaimTypes.AccessAllReleases))
            {
                authContext.Succeed(requirement);
                return;
            }

            var publicationRoles = await _publicationRoleRepository
                .GetAllRolesByUserAndPublicationId(authContext.User.GetUserId(), release.PublicationId);
            var releaseRoles = await _releaseRoleRepository
                .GetAllRolesByUserAndRelease(authContext.User.GetUserId(), release.Id);
            if (ContainPublicationOwnerRole(publicationRoles) || ContainsUnrestrictedViewerRole(releaseRoles))
            {
                authContext.Succeed(requirement);
            }
        }
    }
}

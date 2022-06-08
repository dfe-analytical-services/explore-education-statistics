using System.Threading.Tasks;
using GovUk.Education.ExploreEducationStatistics.Common.Services;
using GovUk.Education.ExploreEducationStatistics.Common.Services.Security;
using GovUk.Education.ExploreEducationStatistics.Content.Model;
using Microsoft.AspNetCore.Authorization;
using static GovUk.Education.ExploreEducationStatistics.Admin.Security.AuthorizationHandlers.AuthorizationHandlerResourceRoleService;
using static GovUk.Education.ExploreEducationStatistics.Content.Model.PublicationRole;
using static GovUk.Education.ExploreEducationStatistics.Content.Model.ReleaseRole;

namespace GovUk.Education.ExploreEducationStatistics.Admin.Security.AuthorizationHandlers
{
    public class ViewSpecificPreReleaseSummaryRequirement : IAuthorizationRequirement
    {}
    
    public class ViewSpecificPreReleaseSummaryAuthorizationHandler 
        : CompoundAuthorizationHandler<ViewSpecificPreReleaseSummaryRequirement, Release>
    {
        public ViewSpecificPreReleaseSummaryAuthorizationHandler(
            AuthorizationHandlerResourceRoleService authorizationHandlerResourceRoleService) 
            : base(
                new CanSeeAllReleasesAuthorizationHandler(),
                new ViewSpecificReleaseAuthorizationHandler.HasUnrestrictedViewerRoleOnReleaseAuthorizationHandler(authorizationHandlerResourceRoleService),
                new HasPreReleaseRoleOnReleaseAuthorizationHandler(authorizationHandlerResourceRoleService))
        {
        }

        public class CanSeeAllReleasesAuthorizationHandler : HasClaimAuthorizationHandler<
            ViewSpecificPreReleaseSummaryRequirement>
        {
            public CanSeeAllReleasesAuthorizationHandler() 
                : base(SecurityClaimTypes.AccessAllReleases) {}
        }
        
        public class HasUnrestrictedViewerRoleOnReleaseAuthorizationHandler
            : AuthorizationHandler<ViewSpecificPreReleaseSummaryRequirement, Release>
        {
            private readonly AuthorizationHandlerResourceRoleService _authorizationHandlerResourceRoleService;

            public HasUnrestrictedViewerRoleOnReleaseAuthorizationHandler(
                AuthorizationHandlerResourceRoleService authorizationHandlerResourceRoleService)
            {
                _authorizationHandlerResourceRoleService = authorizationHandlerResourceRoleService;
            }

            protected override async Task HandleRequirementAsync(
                AuthorizationHandlerContext context,
                ViewSpecificPreReleaseSummaryRequirement requirement,
                Release release)
            {
                if (await _authorizationHandlerResourceRoleService
                        .HasRolesOnPublicationOrRelease(
                            context.User.GetUserId(),
                            release.PublicationId,
                            release.Id,
                            CollectionUtils.ListOf(ReleaseApprover),
                            UnrestrictedReleaseViewerRoles))
                {
                    context.Succeed(requirement);
                }
            }
        }
        
        public class HasPreReleaseRoleOnReleaseAuthorizationHandler
            : AuthorizationHandler<ViewSpecificPreReleaseSummaryRequirement, Release>
        {
            private readonly AuthorizationHandlerResourceRoleService _authorizationHandlerResourceRoleService;

            public HasPreReleaseRoleOnReleaseAuthorizationHandler(
                AuthorizationHandlerResourceRoleService authorizationHandlerResourceRoleService)
            {
                _authorizationHandlerResourceRoleService = authorizationHandlerResourceRoleService;
            }

            protected override async Task HandleRequirementAsync(
                AuthorizationHandlerContext context,
                ViewSpecificPreReleaseSummaryRequirement requirement,
                Release release)
            {
                if (await _authorizationHandlerResourceRoleService
                        .HasRolesOnRelease(
                            context.User.GetUserId(),
                            release.Id,
                            PrereleaseViewer))
                {
                    context.Succeed(requirement);
                }
            }
        }
    }
}

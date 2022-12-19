using System.Threading.Tasks;
using GovUk.Education.ExploreEducationStatistics.Admin.Models;
using GovUk.Education.ExploreEducationStatistics.Admin.Services.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Common.Services.Security;
using GovUk.Education.ExploreEducationStatistics.Content.Model;
using GovUk.Education.ExploreEducationStatistics.Content.Security.AuthorizationHandlers;
using Microsoft.AspNetCore.Authorization;
using static System.DateTime;
using static GovUk.Education.ExploreEducationStatistics.Admin.Security.AuthorizationHandlers.AuthorizationHandlerResourceRoleService;

namespace GovUk.Education.ExploreEducationStatistics.Admin.Security.AuthorizationHandlers
{
    public class ViewSpecificReleaseAuthorizationHandler : CompoundAuthorizationHandler<ViewReleaseRequirement, Release>
    {
        public ViewSpecificReleaseAuthorizationHandler(
            IPreReleaseService preReleaseService,
            AuthorizationHandlerResourceRoleService authorizationHandlerResourceRoleService) 
            : base(
                new CanSeeAllReleasesAuthorizationHandler(),
                new HasOwnerOrApproverRoleOnParentPublicationAuthorizationHandler(authorizationHandlerResourceRoleService),
                new HasUnrestrictedViewerRoleOnReleaseAuthorizationHandler(authorizationHandlerResourceRoleService),
                new HasPreReleaseRoleWithinAccessWindowAuthorizationHandler(preReleaseService, authorizationHandlerResourceRoleService))
        {
        }

        public class CanSeeAllReleasesAuthorizationHandler : HasClaimAuthorizationHandler<
            ViewReleaseRequirement>
        {
            public CanSeeAllReleasesAuthorizationHandler()
                : base(SecurityClaimTypes.AccessAllReleases)
            {
            }
        }
        
        public class HasUnrestrictedViewerRoleOnReleaseAuthorizationHandler
            : AuthorizationHandler<ViewReleaseRequirement, Release>
        {
            private readonly AuthorizationHandlerResourceRoleService _authorizationHandlerResourceRoleService;

            public HasUnrestrictedViewerRoleOnReleaseAuthorizationHandler(
                AuthorizationHandlerResourceRoleService authorizationHandlerResourceRoleService)
            {
                _authorizationHandlerResourceRoleService = authorizationHandlerResourceRoleService;
            }

            protected override async Task HandleRequirementAsync(
                AuthorizationHandlerContext context,
                ViewReleaseRequirement requirement,
                Release release)
            {
                if (await _authorizationHandlerResourceRoleService
                        .HasRolesOnRelease(
                            context.User.GetUserId(),
                            release.Id,
                            UnrestrictedReleaseViewerRoles))
                {
                    context.Succeed(requirement);
                }
            }
        }

        public class HasOwnerOrApproverRoleOnParentPublicationAuthorizationHandler
            : AuthorizationHandler<ViewReleaseRequirement, Release>
        {
            private readonly AuthorizationHandlerResourceRoleService _authorizationHandlerResourceRoleService;

            public HasOwnerOrApproverRoleOnParentPublicationAuthorizationHandler(
                AuthorizationHandlerResourceRoleService authorizationHandlerResourceRoleService)
            {
                _authorizationHandlerResourceRoleService = authorizationHandlerResourceRoleService;
            }

            protected override async Task HandleRequirementAsync(
                AuthorizationHandlerContext context,
                ViewReleaseRequirement requirement,
                Release release)
            {
                if (await _authorizationHandlerResourceRoleService
                        .HasRolesOnPublication(
                            context.User.GetUserId(),
                            release.PublicationId,
                            PublicationRole.Owner, PublicationRole.Approver))
                {
                    context.Succeed(requirement);
                }
            }
        }
        
        public class HasPreReleaseRoleWithinAccessWindowAuthorizationHandler
            : AuthorizationHandler<ViewReleaseRequirement, Release>
        {
            private readonly IPreReleaseService _preReleaseService;
            private readonly AuthorizationHandlerResourceRoleService _authorizationHandlerResourceRoleService;

            public HasPreReleaseRoleWithinAccessWindowAuthorizationHandler(
                IPreReleaseService preReleaseService,
                AuthorizationHandlerResourceRoleService authorizationHandlerResourceRoleService)
            {
                _authorizationHandlerResourceRoleService = authorizationHandlerResourceRoleService;
                _preReleaseService = preReleaseService;
            }

            protected override async Task HandleRequirementAsync(
                AuthorizationHandlerContext context,
                ViewReleaseRequirement requirement,
                Release release)
            {
                if (await _authorizationHandlerResourceRoleService
                        .HasRolesOnRelease(
                            context.User.GetUserId(),
                            release.Id,
                            ReleaseRole.PrereleaseViewer))
                {
                    var windowStatus = _preReleaseService.GetPreReleaseWindowStatus(release, UtcNow);
                    if (windowStatus.Access == PreReleaseAccess.Within)
                    {
                        context.Succeed(requirement);
                    }
                }
            }
        }
    }
}

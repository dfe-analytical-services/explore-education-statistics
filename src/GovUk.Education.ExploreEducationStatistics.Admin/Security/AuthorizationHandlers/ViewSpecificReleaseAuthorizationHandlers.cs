using System.Threading.Tasks;
using GovUk.Education.ExploreEducationStatistics.Admin.Models;
using GovUk.Education.ExploreEducationStatistics.Admin.Services.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Common.Services.Security;
using GovUk.Education.ExploreEducationStatistics.Content.Model;
using GovUk.Education.ExploreEducationStatistics.Content.Security.AuthorizationHandlers;
using Microsoft.AspNetCore.Authorization;
using static System.DateTime;
using static GovUk.Education.ExploreEducationStatistics.Admin.Security.AuthorizationHandlers.AuthorizationHandlerUtil;

namespace GovUk.Education.ExploreEducationStatistics.Admin.Security.AuthorizationHandlers
{
    public class ViewSpecificReleaseAuthorizationHandler : CompoundAuthorizationHandler<ViewReleaseRequirement, Release>
    {
        public ViewSpecificReleaseAuthorizationHandler(
            IUserPublicationRoleRepository userPublicationRoleRepository,
            IUserReleaseRoleRepository userReleaseRoleRepository,
            IPreReleaseService preReleaseService) : base(
            new CanSeeAllReleasesAuthorizationHandler(),
            new HasOwnerRoleOnParentPublicationAuthorizationHandler(userPublicationRoleRepository),
            new HasUnrestrictedViewerRoleOnReleaseAuthorizationHandler(userReleaseRoleRepository),
            new HasPreReleaseRoleWithinAccessWindowAuthorizationHandler(userReleaseRoleRepository, preReleaseService))
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
            : HasRoleOnReleaseAuthorizationHandler<ViewReleaseRequirement>
        {
            public HasUnrestrictedViewerRoleOnReleaseAuthorizationHandler(
                IUserReleaseRoleRepository userReleaseRoleRepository)
                : base(userReleaseRoleRepository, context => ContainsUnrestrictedViewerRole(context.Roles))
            {
            }
        }

        public class HasOwnerRoleOnParentPublicationAuthorizationHandler
            : AuthorizationHandler<ViewReleaseRequirement, Release>
        {
            private readonly IUserPublicationRoleRepository _userPublicationRoleRepository;

            public HasOwnerRoleOnParentPublicationAuthorizationHandler(
                IUserPublicationRoleRepository userPublicationRoleRepository)
            {
                _userPublicationRoleRepository = userPublicationRoleRepository;
            }

            protected override async Task HandleRequirementAsync(
                AuthorizationHandlerContext context,
                ViewReleaseRequirement requirement,
                Release release)
            {
                var publicationRoles =
                    await _userPublicationRoleRepository.GetAllRolesByUserAndPublication(context.User.GetUserId(),
                        release.PublicationId);

                if (ContainPublicationOwnerRole(publicationRoles))
                {
                    context.Succeed(requirement);
                }
            }
        }

        public class HasPreReleaseRoleWithinAccessWindowAuthorizationHandler
            : HasRoleOnReleaseAuthorizationHandler<ViewReleaseRequirement>
        {
            public HasPreReleaseRoleWithinAccessWindowAuthorizationHandler(
                IUserReleaseRoleRepository userReleaseRoleRepository, IPreReleaseService preReleaseService)
                : base(userReleaseRoleRepository, context =>
                {
                    if (!ContainsPreReleaseViewerRole(context.Roles))
                    {
                        return false;
                    }

                    var windowStatus = preReleaseService.GetPreReleaseWindowStatus(context.Release, UtcNow);
                    return windowStatus.Access == PreReleaseAccess.Within;
                })
            {
            }
        }
    }
}

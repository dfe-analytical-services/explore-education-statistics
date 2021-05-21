using GovUk.Education.ExploreEducationStatistics.Admin.Models;
using GovUk.Education.ExploreEducationStatistics.Admin.Services.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Content.Model;
using GovUk.Education.ExploreEducationStatistics.Content.Security.AuthorizationHandlers;
using static System.DateTime;
using static GovUk.Education.ExploreEducationStatistics.Admin.Security.AuthorizationHandlers.AuthorizationHandlerUtil;

namespace GovUk.Education.ExploreEducationStatistics.Admin.Security.AuthorizationHandlers
{
    public class ViewSpecificReleaseAuthorizationHandler : CompoundAuthorizationHandler<ViewReleaseRequirement, Release>
    {
        public ViewSpecificReleaseAuthorizationHandler(
            IUserReleaseRoleRepository userReleaseRoleRepository, IPreReleaseService preReleaseService) : base(
            new CanSeeAllReleasesAuthorizationHandler(),
            new HasUnrestrictedViewerRoleOnReleaseAuthorizationHandler(userReleaseRoleRepository),
            new HasPreReleaseRoleWithinAccessWindowAuthorizationHandler(userReleaseRoleRepository, preReleaseService))
        {
        }

        public class CanSeeAllReleasesAuthorizationHandler : HasClaimAuthorizationHandler<
            ViewReleaseRequirement>
        {
            public CanSeeAllReleasesAuthorizationHandler()
                : base(SecurityClaimTypes.AccessAllReleases) {}
        }

        public class HasUnrestrictedViewerRoleOnReleaseAuthorizationHandler
            : HasRoleOnReleaseAuthorizationHandler<ViewReleaseRequirement>
        {
            public HasUnrestrictedViewerRoleOnReleaseAuthorizationHandler(IUserReleaseRoleRepository userReleaseRoleRepository)
                : base(userReleaseRoleRepository, context => ContainsUnrestrictedViewerRole(context.Roles))
            {}
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
            {}
        }
    }
}

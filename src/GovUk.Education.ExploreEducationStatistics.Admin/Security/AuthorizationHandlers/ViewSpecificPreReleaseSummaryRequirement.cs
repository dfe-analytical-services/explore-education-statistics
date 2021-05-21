using GovUk.Education.ExploreEducationStatistics.Admin.Services.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Content.Model;
using Microsoft.AspNetCore.Authorization;
using static GovUk.Education.ExploreEducationStatistics.Admin.Security.AuthorizationHandlers.AuthorizationHandlerUtil;

namespace GovUk.Education.ExploreEducationStatistics.Admin.Security.AuthorizationHandlers
{
    public class ViewSpecificPreReleaseSummaryRequirement : IAuthorizationRequirement
    {}
    
    public class ViewSpecificPreReleaseSummaryAuthorizationHandler : CompoundAuthorizationHandler<ViewSpecificPreReleaseSummaryRequirement, Release>
    {
        public ViewSpecificPreReleaseSummaryAuthorizationHandler(IUserReleaseRoleRepository userReleaseRoleRepository) : base(
            new CanSeeAllReleasesAuthorizationHandler(),
            new HasUnrestrictedViewerRoleOnReleaseAuthorizationHandler(userReleaseRoleRepository),
            new HasPreReleaseRoleOnReleaseAuthorizationHandler(userReleaseRoleRepository))
        {
        }

        public class CanSeeAllReleasesAuthorizationHandler : HasClaimAuthorizationHandler<
            ViewSpecificPreReleaseSummaryRequirement>
        {
            public CanSeeAllReleasesAuthorizationHandler() 
                : base(SecurityClaimTypes.AccessAllReleases) {}
        }
    
        public class HasUnrestrictedViewerRoleOnReleaseAuthorizationHandler
            : HasRoleOnReleaseAuthorizationHandler<ViewSpecificPreReleaseSummaryRequirement>
        {
            public HasUnrestrictedViewerRoleOnReleaseAuthorizationHandler(IUserReleaseRoleRepository userReleaseRoleRepository) 
                : base(userReleaseRoleRepository, context => ContainsUnrestrictedViewerRole(context.Roles))
            {}
        }

        public class HasPreReleaseRoleOnReleaseAuthorizationHandler
            : HasRoleOnReleaseAuthorizationHandler<ViewSpecificPreReleaseSummaryRequirement>
        {
            public HasPreReleaseRoleOnReleaseAuthorizationHandler(IUserReleaseRoleRepository userReleaseRoleRepository)
                : base(userReleaseRoleRepository, context => ContainsPreReleaseViewerRole(context.Roles))
            {
            }
        }
    }
}

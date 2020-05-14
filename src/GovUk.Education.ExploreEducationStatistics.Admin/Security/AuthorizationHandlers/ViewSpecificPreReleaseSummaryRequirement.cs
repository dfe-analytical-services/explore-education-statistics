using GovUk.Education.ExploreEducationStatistics.Content.Model;
using GovUk.Education.ExploreEducationStatistics.Content.Model.Database;
using Microsoft.AspNetCore.Authorization;
using static GovUk.Education.ExploreEducationStatistics.Admin.Security.AuthorizationHandlers.AuthorizationHandlerUtil;

namespace GovUk.Education.ExploreEducationStatistics.Admin.Security.AuthorizationHandlers
{
    public class ViewSpecificPreReleaseSummaryRequirement : IAuthorizationRequirement
    {}
    
    public class ViewSpecificPreReleaseSummaryAuthorizationHandler : CompoundAuthorizationHandler<ViewSpecificPreReleaseSummaryRequirement, Release>
    {
        public ViewSpecificPreReleaseSummaryAuthorizationHandler(ContentDbContext context) : base(
            new CanSeeAllReleasesAuthorizationHandler(),
            new HasUnrestrictedViewerRoleOnReleaseAuthorizationHandler(context),
            new HasPreReleaseRoleOnReleaseAuthorizationHandler(context))
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
            public HasUnrestrictedViewerRoleOnReleaseAuthorizationHandler(ContentDbContext context) 
                : base(context, ctx => ContainsUnrestrictedViewerRole(ctx.Roles))
            {}
        }

        public class HasPreReleaseRoleOnReleaseAuthorizationHandler
            : HasRoleOnReleaseAuthorizationHandler<ViewSpecificPreReleaseSummaryRequirement>
        {
            public HasPreReleaseRoleOnReleaseAuthorizationHandler(ContentDbContext context)
                : base(context, ctx => ContainsPreReleaseViewerRole(ctx.Roles))
            {
            }
        }
    }
}
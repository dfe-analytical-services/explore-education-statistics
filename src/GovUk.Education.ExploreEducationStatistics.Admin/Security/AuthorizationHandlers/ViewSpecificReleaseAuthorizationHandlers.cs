using GovUk.Education.ExploreEducationStatistics.Admin.Models;
using GovUk.Education.ExploreEducationStatistics.Admin.Services.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Content.Model;
using GovUk.Education.ExploreEducationStatistics.Content.Model.Database;
using GovUk.Education.ExploreEducationStatistics.Content.Security.AuthorizationHandlers;
using static System.DateTime;
using static GovUk.Education.ExploreEducationStatistics.Admin.Security.AuthorizationHandlers.AuthorizationHandlerUtil;

namespace GovUk.Education.ExploreEducationStatistics.Admin.Security.AuthorizationHandlers
{
    public class ViewSpecificReleaseAuthorizationHandler : CompoundAuthorizationHandler<ViewReleaseRequirement, Release>
    {
        public ViewSpecificReleaseAuthorizationHandler(
            ContentDbContext context, IPreReleaseService preReleaseService) : base(
            new CanSeeAllReleasesAuthorizationHandler(),
            new HasUnrestrictedViewerRoleOnReleaseAuthorizationHandler(context),
            new HasPreReleaseRoleWithinAccessWindowAuthorizationHandler(context, preReleaseService))
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
            public HasUnrestrictedViewerRoleOnReleaseAuthorizationHandler(ContentDbContext context)
                : base(context, ctx => ContainsUnrestrictedViewerRole(ctx.Roles))
            {}
        }

        public class HasPreReleaseRoleWithinAccessWindowAuthorizationHandler
            : HasRoleOnReleaseAuthorizationHandler<ViewReleaseRequirement>
        {
            public HasPreReleaseRoleWithinAccessWindowAuthorizationHandler(
                ContentDbContext context, IPreReleaseService preReleaseService)
                : base(context, ctx =>
                {
                    if (!ContainsPreReleaseViewerRole(ctx.Roles))
                    {
                        return false;
                    }

                    var windowStatus = preReleaseService.GetPreReleaseWindowStatus(ctx.Release, UtcNow);
                    return windowStatus.Access == PreReleaseAccess.Within;
                })
            {}
        }
    }
}
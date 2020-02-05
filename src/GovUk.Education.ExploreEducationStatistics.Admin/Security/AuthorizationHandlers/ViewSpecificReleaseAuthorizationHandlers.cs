using GovUk.Education.ExploreEducationStatistics.Admin.Services.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Content.Model;
using GovUk.Education.ExploreEducationStatistics.Content.Model.Database;
using Microsoft.AspNetCore.Authorization;
using static System.DateTime;
using static GovUk.Education.ExploreEducationStatistics.Admin.Security.AuthorizationHandlers.AuthorizationHandlerUtil;

namespace GovUk.Education.ExploreEducationStatistics.Admin.Security.AuthorizationHandlers
{
    public class ViewSpecificReleaseRequirement : IAuthorizationRequirement
    {}
    
    public class ViewSpecificReleaseAuthorizationHandler : CompoundAuthorizationHandler<ViewSpecificReleaseRequirement, Release>
    {
        public ViewSpecificReleaseAuthorizationHandler(
            ContentDbContext context, IPreReleaseService preReleaseService) : base(
            new CanSeeAllReleasesAuthorizationHandler(),
            new HasUnrestrictedViewerRoleOnReleaseAuthorizationHandler(context),
            new HasPreReleaseRoleWithinAccessWindowAuthorizationHandler(context, preReleaseService))
        {
            
        }
    }
    
    public class CanSeeAllReleasesAuthorizationHandler : HasClaimAuthorizationHandler<
            ViewSpecificReleaseRequirement>
    {
        public CanSeeAllReleasesAuthorizationHandler() 
            : base(SecurityClaimTypes.AccessAllReleases) {}
    }
    
    public class HasUnrestrictedViewerRoleOnReleaseAuthorizationHandler
        : HasRoleOnReleaseAuthorizationHandler<ViewSpecificReleaseRequirement>
    {
        public HasUnrestrictedViewerRoleOnReleaseAuthorizationHandler(ContentDbContext context) 
            : base(context, ctx => ContainsUnrestrictedViewerRole(ctx.Roles))
        {}
    }
    
    public class HasPreReleaseRoleWithinAccessWindowAuthorizationHandler
        : HasRoleOnReleaseAuthorizationHandler<ViewSpecificReleaseRequirement>
    {
        public HasPreReleaseRoleWithinAccessWindowAuthorizationHandler(
            ContentDbContext context, IPreReleaseService preReleaseService) 
            : base(context, ctx =>
            {
                if (!ContainsPreReleaseViewerRole(ctx.Roles))
                {
                    return false;
                }

                var windowStatus = preReleaseService.GetPreReleaseWindowStatus(ctx.Release, Now);
                return windowStatus.PreReleaseAccess == PreReleaseAccess.Within;
            })
        {}
    }
}
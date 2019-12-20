using GovUk.Education.ExploreEducationStatistics.Content.Model.Database;
using Microsoft.AspNetCore.Authorization;

namespace GovUk.Education.ExploreEducationStatistics.Admin.Security.AuthorizationHandlers
{
    public class ViewSpecificReleaseRequirement : IAuthorizationRequirement
    {}
    
    public class ViewSpecificReleaseCanSeeAllReleasesAuthorizationHandler : HasClaimAuthorizationHandler<
            ViewSpecificReleaseRequirement>
    {
        public ViewSpecificReleaseCanSeeAllReleasesAuthorizationHandler() 
            : base(SecurityClaimTypes.AccessAllReleases) {}
    }
    
    public class ViewSpecificReleaseHasRoleOnReleaseAuthorizationHandler
        : HasRoleOnReleaseAuthorizationHandler<ViewSpecificReleaseRequirement>
    {
        public ViewSpecificReleaseHasRoleOnReleaseAuthorizationHandler(ContentDbContext context) : base(context)
        {}
    }
}
using System.Linq;
using System.Threading.Tasks;
using GovUk.Education.ExploreEducationStatistics.Content.Model;
using GovUk.Education.ExploreEducationStatistics.Content.Model.Database;
using Microsoft.AspNetCore.Authorization;
using static GovUk.Education.ExploreEducationStatistics.Admin.Security.SecurityUtils;

namespace GovUk.Education.ExploreEducationStatistics.Admin.Security
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
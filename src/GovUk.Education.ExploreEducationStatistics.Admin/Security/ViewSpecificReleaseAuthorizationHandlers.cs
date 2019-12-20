using System.Linq;
using System.Threading.Tasks;
using GovUk.Education.ExploreEducationStatistics.Content.Model;
using GovUk.Education.ExploreEducationStatistics.Content.Model.Database;
using Microsoft.AspNetCore.Authorization;
using static GovUk.Education.ExploreEducationStatistics.Admin.Security.SecurityUtils;

namespace GovUk.Education.ExploreEducationStatistics.Admin.Security
{
    public class ViewSpecificReleaseCanSeeAllReleasesAuthorizationHandler : HasClaimAuthorizationHandler<
            ViewSpecificReleaseRequirement>
    {
        public ViewSpecificReleaseCanSeeAllReleasesAuthorizationHandler() 
            : base(SecurityClaimTypes.AccessAllReleases) {}
    }
    
    public class ViewSpecificReleaseHasRoleOnReleaseAuthorizationHandler 
        : AuthorizationHandler<ViewSpecificReleaseRequirement, Release>
    {
        private readonly ContentDbContext _context;

        public ViewSpecificReleaseHasRoleOnReleaseAuthorizationHandler(ContentDbContext context)
        {
            _context = context;
        }

        protected override Task HandleRequirementAsync(AuthorizationHandlerContext authContext,
            ViewSpecificReleaseRequirement requirement,
            Release release)
        {
            var userId = GetUserId(authContext.User);

            var connectedToRelease = _context
                .UserReleaseRoles
                .Any(r => r.ReleaseId == release.Id && r.UserId == userId);
        
            if (connectedToRelease)
            {
                authContext.Succeed(requirement);
            }

            return Task.CompletedTask;
        }
    }
}
using System.Linq;
using System.Threading.Tasks;
using GovUk.Education.ExploreEducationStatistics.Common.Security.AuthorizationHandlers;
using GovUk.Education.ExploreEducationStatistics.Common.Services.Security;
using GovUk.Education.ExploreEducationStatistics.Content.Model;
using GovUk.Education.ExploreEducationStatistics.Content.Model.Database;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;

namespace GovUk.Education.ExploreEducationStatistics.Admin.Security.AuthorizationHandlers
{
    public class ViewSpecificPublicationRequirement : IAuthorizationRequirement
    {}
    
    public class ViewSpecificPublicationAuthorizationHandler : CompoundAuthorizationHandler<ViewSpecificPublicationRequirement, Publication>
    {
        public ViewSpecificPublicationAuthorizationHandler(ContentDbContext context) : base(
            new CanSeeAllPublicationsAuthorizationHandler(),
            new HasRoleOnAnyChildReleaseAuthorizationHandler(context))
        {
            
        }
    }
    
    public class CanSeeAllPublicationsAuthorizationHandler : HasClaimAuthorizationHandler<
            ViewSpecificPublicationRequirement>
    {
        public CanSeeAllPublicationsAuthorizationHandler() 
            : base(SecurityClaimTypes.AccessAllReleases) {}
    }
    
    public class HasRoleOnAnyChildReleaseAuthorizationHandler
        : AuthorizationHandler<ViewSpecificPublicationRequirement, Publication>
    {
        private readonly ContentDbContext _context;

        public HasRoleOnAnyChildReleaseAuthorizationHandler(ContentDbContext context)
        {
            _context = context;
        }

        protected override async Task HandleRequirementAsync(AuthorizationHandlerContext authContext, 
            ViewSpecificPublicationRequirement requirement, Publication publication)
        {
            var userId = authContext.User.GetUserId();
            
            if (await _context
                .UserReleaseRoles
                .Include(r => r.Release)
                .Where(r => r.UserId == userId)
                .AnyAsync(r => r.Release.PublicationId == publication.Id))
            {
                authContext.Succeed(requirement);
            }
        }
    }
}
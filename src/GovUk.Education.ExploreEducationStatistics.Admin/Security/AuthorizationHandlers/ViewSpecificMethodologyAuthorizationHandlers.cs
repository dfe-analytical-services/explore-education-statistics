using System.Linq;
using System.Threading.Tasks;
using GovUk.Education.ExploreEducationStatistics.Common.Services.Security;
using GovUk.Education.ExploreEducationStatistics.Content.Model;
using GovUk.Education.ExploreEducationStatistics.Content.Model.Database;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;

namespace GovUk.Education.ExploreEducationStatistics.Admin.Security.AuthorizationHandlers
{
    public class ViewSpecificMethodologyRequirement : IAuthorizationRequirement
    {}

    public class ViewSpecificMethodologyAuthorizationHandler
        : CompoundAuthorizationHandler<ViewSpecificMethodologyRequirement, Methodology>
    {
        public ViewSpecificMethodologyAuthorizationHandler(ContentDbContext context) : base(
            new CanViewAllMethodologiesAuthorizationHandler(),
            new HasRoleOnAnyAssociatedReleaseAuthorizationHandler(context)) {}
    
        public class CanViewAllMethodologiesAuthorizationHandler 
            : HasClaimAuthorizationHandler<ViewSpecificMethodologyRequirement>
        {
            public CanViewAllMethodologiesAuthorizationHandler()
                : base(SecurityClaimTypes.AccessAllMethodologies) {}
        }

        public class HasRoleOnAnyAssociatedReleaseAuthorizationHandler
            : AuthorizationHandler<ViewSpecificMethodologyRequirement, Methodology>
        {
            private readonly ContentDbContext _context;
            public HasRoleOnAnyAssociatedReleaseAuthorizationHandler(ContentDbContext context)
            {
                _context = context;
            }

            protected override async Task HandleRequirementAsync(
                AuthorizationHandlerContext authContext,
                ViewSpecificMethodologyRequirement requirement,
                Methodology methodology)
            {
                if (methodology.Publications == null)
                {
                    methodology = await _context.Methodologies
                        .Include(m => m.Publications)
                        .Where(m => m.Id == methodology.Id)
                        .SingleAsync();
                }
                
                var userId = authContext.User.GetUserId();
                var viewablePublications = await _context.UserReleaseRoles
                    .Include(urr => urr.Release)
                    .ThenInclude(r => r.Publication)
                    .Where(urr => 
                        urr.UserId == userId
                        && urr.Role != ReleaseRole.PrereleaseViewer)
                    .Select(urr => urr.Release.Publication)
                    .Distinct()
                    .ToListAsync();

                if (viewablePublications
                    .Any(p1 => methodology.Publications
                        .Select(p2 => p2.Id)
                        .Contains(p1.Id)))
                {
                    authContext.Succeed(requirement);
                }
            }
        }
    }
}

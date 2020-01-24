using System.Linq;
using System.Threading.Tasks;
using GovUk.Education.ExploreEducationStatistics.Content.Model;
using GovUk.Education.ExploreEducationStatistics.Content.Model.Database;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;

namespace GovUk.Education.ExploreEducationStatistics.Admin.Security.AuthorizationHandlers
{
    public class ViewSpecificThemeRequirement : IAuthorizationRequirement
    {}
    
    public class ViewSpecificThemeAuthorizationHandler : CompoundAuthorizationHandler<ViewSpecificThemeRequirement, Theme>
    {
        public ViewSpecificThemeAuthorizationHandler(ContentDbContext context) : base(
            new CanSeeAllThemesAuthorizationHandler(),
            new HasRoleOnAnyChildPublicationAuthorizationHandler(context))
        {
            
        }
    }
    
    public class CanSeeAllThemesAuthorizationHandler : HasClaimAuthorizationHandler<
            ViewSpecificThemeRequirement>
    {
        public CanSeeAllThemesAuthorizationHandler() 
            : base(SecurityClaimTypes.AccessAllTopics) {}
    }
    
    public class HasRoleOnAnyChildPublicationAuthorizationHandler
        : AuthorizationHandler<ViewSpecificThemeRequirement, Theme>
    {
        private readonly ContentDbContext _context;

        public HasRoleOnAnyChildPublicationAuthorizationHandler(ContentDbContext context)
        {
            _context = context;
        }

        protected override async Task HandleRequirementAsync(AuthorizationHandlerContext authContext, 
            ViewSpecificThemeRequirement requirement, Theme theme)
        {
            var userId = SecurityUtils.GetUserId(authContext.User);
            
            if (await _context
                .UserReleaseRoles
                .Include(r => r.Release)
                .ThenInclude(r => r.Publication)
                .ThenInclude(p => p.Topic)
                .Where(r => r.UserId == userId)
                .AnyAsync(r => r.Release.Publication.Topic.ThemeId == theme.Id))
            {
                authContext.Succeed(requirement);
            }
        }
    }
}
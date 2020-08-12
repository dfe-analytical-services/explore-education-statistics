
using System.Linq;
using System.Threading.Tasks;
using GovUk.Education.ExploreEducationStatistics.Common.Services.Security;
using GovUk.Education.ExploreEducationStatistics.Content.Model;
using GovUk.Education.ExploreEducationStatistics.Content.Model.Database;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;

namespace GovUk.Education.ExploreEducationStatistics.Admin.Security.AuthorizationHandlers
{
    public class ViewSpecificTopicRequirement : IAuthorizationRequirement
    {}
    
    public class ViewSpecificTopicAuthorizationHandler : CompoundAuthorizationHandler<ViewSpecificTopicRequirement, Topic>
    {
        public ViewSpecificTopicAuthorizationHandler(ContentDbContext context) : base(
            new CanViewAllTopics(),
            new HasRoleOnAnyChildRelease(context))
        {
        }

        public class CanViewAllTopics : HasClaimAuthorizationHandler<
                ViewSpecificTopicRequirement>
        {
            public CanViewAllTopics() 
                : base(SecurityClaimTypes.AccessAllTopics) {}
        }
    
        public class HasRoleOnAnyChildRelease
            : AuthorizationHandler<ViewSpecificTopicRequirement, Topic>
        {
            private readonly ContentDbContext _context;

            public HasRoleOnAnyChildRelease(ContentDbContext context)
            {
                _context = context;
            }

            protected override async Task HandleRequirementAsync(AuthorizationHandlerContext authContext, 
                ViewSpecificTopicRequirement requirement, Topic topic)
            {
                var userId = authContext.User.GetUserId();
            
                if (await _context
                    .UserReleaseRoles
                    .Include(r => r.Release)
                    .ThenInclude(r => r.Publication)
                    .ThenInclude(p => p.Topic)
                    .Where(r => r.UserId == userId)
                    .AnyAsync(r => r.Release.Publication.Topic.Id == topic.Id))
                {
                    authContext.Succeed(requirement);
                }
            }
        }
    }
}
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using GovUk.Education.ExploreEducationStatistics.Content.Model;
using GovUk.Education.ExploreEducationStatistics.Content.Model.Database;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;

namespace GovUk.Education.ExploreEducationStatistics.Admin.Security.AuthorizationHandlers
{
    public class ReleaseAuthorizationContext
    {
        public ReleaseAuthorizationContext(Release release, List<UserReleaseRole> roles)
        {
            Release = release;
            Roles = roles;
        }

        public Release Release { get; set; }

        public List<UserReleaseRole> Roles { get; set; }
    }
    
    public abstract class HasRoleOnReleaseAuthorizationHandler<TRequirement> : AuthorizationHandler<TRequirement, Release> 
        where TRequirement : IAuthorizationRequirement
    {
        private readonly ContentDbContext _context;
        private readonly Predicate<ReleaseAuthorizationContext>? _roleTest;

        protected HasRoleOnReleaseAuthorizationHandler(ContentDbContext context, Predicate<ReleaseAuthorizationContext> roleTest = null)
        {
            _context = context;
            _roleTest = roleTest;
        }

        protected override async Task HandleRequirementAsync(AuthorizationHandlerContext authContext,
            TRequirement requirement,
            Release release)
        {
            var userId = SecurityUtils.GetUserId(authContext.User);

            var releaseRoles = await _context
                .UserReleaseRoles
                .Where(r => r.ReleaseId == release.Id && r.UserId == userId)
                .ToListAsync();
        
            if (releaseRoles.Any() && (_roleTest == null || _roleTest.Invoke(new ReleaseAuthorizationContext(release, releaseRoles)))) 
            {
                authContext.Succeed(requirement);    
            }
        }
    }
}
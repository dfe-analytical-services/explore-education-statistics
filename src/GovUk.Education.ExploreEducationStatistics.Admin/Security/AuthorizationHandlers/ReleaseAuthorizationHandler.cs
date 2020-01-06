using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using GovUk.Education.ExploreEducationStatistics.Content.Model;
using GovUk.Education.ExploreEducationStatistics.Content.Model.Database;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;

namespace GovUk.Education.ExploreEducationStatistics.Admin.Security.AuthorizationHandlers
{
    public class ReleaseAuthorizationContext
    {
        public Release Release { get; set; }
        
        public ClaimsPrincipal User { get; set; }
        
        public ReleaseAuthorizationContext(Release release, ClaimsPrincipal user)
        {
            Release = release;
            User = user;
        }
    }
    
    public abstract class ReleaseAuthorizationHandler<TRequirement> : AuthorizationHandler<TRequirement, Release> 
        where TRequirement : IAuthorizationRequirement
    {
        private readonly Predicate<ReleaseAuthorizationContext> _releaseTest;

        protected ReleaseAuthorizationHandler(Predicate<ReleaseAuthorizationContext> releaseTest)
        {
            _releaseTest = releaseTest;
        }

        protected override Task HandleRequirementAsync(AuthorizationHandlerContext authContext,
            TRequirement requirement,
            Release release)
        {
            if (_releaseTest.Invoke(new ReleaseAuthorizationContext(release, authContext.User))) 
            {
                authContext.Succeed(requirement);    
            }

            return Task.CompletedTask;
        }
    }
}
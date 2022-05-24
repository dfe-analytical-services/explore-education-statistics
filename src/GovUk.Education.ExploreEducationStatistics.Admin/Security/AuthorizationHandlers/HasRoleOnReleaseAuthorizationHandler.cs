using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using GovUk.Education.ExploreEducationStatistics.Admin.Services.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Common.Services.Security;
using GovUk.Education.ExploreEducationStatistics.Content.Model;
using Microsoft.AspNetCore.Authorization;

namespace GovUk.Education.ExploreEducationStatistics.Admin.Security.AuthorizationHandlers
{
    public abstract class HasRoleOnReleaseAuthorizationHandler<TRequirement> : AuthorizationHandler<TRequirement, Release> 
        where TRequirement : IAuthorizationRequirement
    {
        private readonly IUserReleaseRoleRepository _releaseRoleRepository;
        private readonly Predicate<ReleaseRolesAuthorizationContext> _test;

        protected HasRoleOnReleaseAuthorizationHandler(IUserReleaseRoleRepository releaseRoleRepository,
            Predicate<ReleaseRolesAuthorizationContext> test)
        {
            _releaseRoleRepository = releaseRoleRepository;
            _test = test;
        }

        protected override async Task HandleRequirementAsync(AuthorizationHandlerContext context,
            TRequirement requirement,
            Release release)
        {
            var releaseRoles = await _releaseRoleRepository.GetDistinctRolesByUserAndRelease(context.User.GetUserId(), release.Id);

            if (releaseRoles.Any())
            {
                if (_test == null || _test.Invoke(new ReleaseRolesAuthorizationContext(release, releaseRoles)))
                {
                    context.Succeed(requirement);   
                }
            }
        }
    }
    
    public class ReleaseRolesAuthorizationContext
    {
        public ReleaseRolesAuthorizationContext(Release release, List<ReleaseRole> roles)
        {
            Release = release;
            Roles = roles;
        }

        public Release Release { get; set; }

        public List<ReleaseRole> Roles { get; set; }
    }
}

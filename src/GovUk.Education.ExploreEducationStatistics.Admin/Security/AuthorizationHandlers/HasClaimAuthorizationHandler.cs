using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;

namespace GovUk.Education.ExploreEducationStatistics.Admin.Security.AuthorizationHandlers
{
    public abstract class HasClaimAuthorizationHandler<TRequirement> : AuthorizationHandler<TRequirement> 
        where TRequirement : IAuthorizationRequirement
    {
        private readonly string _claimType;
        private readonly string? _claimValue;

        protected HasClaimAuthorizationHandler(SecurityClaimTypes claimType, string? claimValue = null)
        {
            _claimType = claimType.ToString();
            _claimValue = claimValue;
        }

        protected override Task HandleRequirementAsync(AuthorizationHandlerContext authContext,
            TRequirement requirement)
        {
            if (authContext.User.HasClaim(
                c => c.Type == _claimType && (_claimValue == null || c.Value == _claimValue)))
            {
                authContext.Succeed(requirement);
            }

            return Task.CompletedTask;
        }
    }
}
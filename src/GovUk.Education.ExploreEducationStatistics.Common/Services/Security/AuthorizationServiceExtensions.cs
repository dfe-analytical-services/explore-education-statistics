using System;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;

namespace GovUk.Education.ExploreEducationStatistics.Common.Services.Security
{
    public static class AuthorizationServiceExtensions
    {
        public static async Task<bool> MatchesPolicy(
            this IAuthorizationService authorizationService,
            ClaimsPrincipal user, 
            Enum policy
        )
        {
            var result = await authorizationService.AuthorizeAsync(user, policy.ToString());
            return result.Succeeded;
        }
        
        public static async Task<bool> MatchesPolicy(
            this IAuthorizationService authorizationService,
            ClaimsPrincipal user, 
            object resource,
            Enum policy
        )
        {
            var result = await authorizationService.AuthorizeAsync(user, resource, policy.ToString());
            return result.Succeeded;
        }
    }
}
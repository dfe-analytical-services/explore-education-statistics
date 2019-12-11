using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;

namespace GovUk.Education.ExploreEducationStatistics.Admin.Security
{
    public static class AuthorizationServiceExtensions
    {
        public static bool MatchesPolicy(
            this IAuthorizationService authorizationService,
            ClaimsPrincipal user, 
            SecurityPolicies policy
        )
        {
            return authorizationService.AuthorizeAsync(user, policy.ToString()).Result.Succeeded;
        }
    }
}
using System;
using System.Linq;
using System.Security.Claims;
using GovUk.Education.ExploreEducationStatistics.Admin.Security.AuthorizationHandlers;

namespace GovUk.Education.ExploreEducationStatistics.Admin.Security
{
    public class SecurityUtils
    {
        public static Guid GetUserId(ClaimsPrincipal principal)
        {
            var userIdClaim = principal
                .Claims
                .First(claim => claim.Type == ClaimTypes.NameIdentifier);
            
            return new Guid(userIdClaim.Value);
        }

        public static bool HasClaim(ClaimsPrincipal user, SecurityClaimTypes claimType, string? claimValue = null)
        {
            return user.HasClaim(
                c => c.Type == claimType.ToString() && (claimValue == null || c.Value == claimValue));
        }
    }
}
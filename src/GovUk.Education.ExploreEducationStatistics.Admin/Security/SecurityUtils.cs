using System.Security.Claims;

namespace GovUk.Education.ExploreEducationStatistics.Admin.Security
{
    public class SecurityUtils
    {
        public static bool HasClaim(ClaimsPrincipal user, SecurityClaimTypes claimType, string? claimValue = null)
        {
            return user.HasClaim(
                c => c.Type == claimType.ToString() && (claimValue == null || c.Value == claimValue));
        }
    }
}
using System;
using System.Linq;
using System.Security.Claims;

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
    }
}
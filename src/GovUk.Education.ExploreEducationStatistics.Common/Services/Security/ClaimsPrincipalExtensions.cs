using System;
using System.Linq;
using System.Security.Claims;
using GovUk.Education.ExploreEducationStatistics.Admin.Security;

namespace GovUk.Education.ExploreEducationStatistics.Common.Services.Security
{
    public static class ClaimsPrincipalExtensions
    {
        public static Guid GetUserId(this ClaimsPrincipal principal)
        {
            // Look up the current User's "internal" service User id (from the Users table) if it has been set already
            // on their JWT
            var internalUserIdClaim = principal
                .Claims
                .FirstOrDefault(claim => claim.Type == UserClaimTypes.InternalUserId.ToString());

            if (internalUserIdClaim != null)
            {
                return Guid.Parse(internalUserIdClaim.Value);
            }
            
            // As a fallback, look up their original Identity Framework User id from the AspNetUsers table, which
            // historically will always be set and match a corresponding Users record with an identical id.  Eventually
            // this behaviour will expire as all active JWTs will be issued with the above "internal" User Id set. 
            var identityUserIdClaim = principal
                .Claims
                .First(claim => claim.Type == ClaimTypes.NameIdentifier);

            return Guid.Parse(identityUserIdClaim.Value);
        }
    }
}
using System;
using System.Linq;
using System.Security.Claims;

namespace GovUk.Education.ExploreEducationStatistics.Common.Services.Security
{
    public static class ClaimsPrincipalExtensions
    {
        public static Guid GetUserId(this ClaimsPrincipal principal)
        {
            var userIdClaim = principal
                .Claims
                .First(claim => claim.Type == ClaimTypes.NameIdentifier);

            return Guid.Parse(userIdClaim.Value);
        }
    }
}
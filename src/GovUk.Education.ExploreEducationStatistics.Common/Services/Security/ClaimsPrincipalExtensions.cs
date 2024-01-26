using System;
using System.Linq;
using System.Security.Claims;

namespace GovUk.Education.ExploreEducationStatistics.Common.Services.Security
{
    public static class ClaimsPrincipalExtensions
    {
        public const string EesUserIdClaim = "";

        public static Guid GetUserId(this ClaimsPrincipal principal)
        {
            var userIdClaim = principal.FindFirstValue(EesUserIdClaim);
            return Guid.Parse(userIdClaim);
        }
    }
}

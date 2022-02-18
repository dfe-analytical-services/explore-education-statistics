using System;
using System.Linq;
using System.Security.Claims;
using GovUk.Education.ExploreEducationStatistics.Admin.Security;

namespace GovUk.Education.ExploreEducationStatistics.Admin.Tests.Security.Utils
{
    public static class ClaimsPrincipalUtils
    {
        public static ClaimsPrincipal CreateClaimsPrincipal(Guid userId)
        {
            return CreateClaimsPrincipal(userId, new Claim[] { });
        }

        public static ClaimsPrincipal CreateClaimsPrincipal(Guid userId, params Claim[] additionalClaims)
        {
            var identity = new ClaimsIdentity();
            identity.AddClaim(new Claim(ClaimTypes.NameIdentifier, userId.ToString()));
            identity.AddClaims(additionalClaims);
            var user = new ClaimsPrincipal(identity);
            return user;
        }

        public static ClaimsPrincipal CreateClaimsPrincipal(Guid userId, params SecurityClaimTypes[] additionalClaims)
        {
            return CreateClaimsPrincipal(userId,
                additionalClaims.Select(c => new Claim(c.ToString(), "")).ToArray());
        }
    }
}
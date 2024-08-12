using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using GovUk.Education.ExploreEducationStatistics.Common.Services.Security;

namespace GovUk.Education.ExploreEducationStatistics.Common.Tests.Utils;

public static class ClaimsPrincipalUtils
{
    public static ClaimsPrincipal CreateClaimsPrincipal(
        params Claim[] claims)
    {
        var identity = new ClaimsIdentity(
            claims: new List<Claim>(),
            authenticationType: "Bearer",
            nameType: EesClaimTypes.Name,
            roleType: EesClaimTypes.Role);

        identity.AddClaims(claims);
        var user = new ClaimsPrincipal(identity);
        return user;
    }

    /// <summary>
    /// Create a Claim representing a Role.
    /// </summary>
    public static Claim RoleClaim(string roleName)
    {
        return new Claim(EesClaimTypes.Role, roleName);
    }

    /// <summary>
    /// Create a Claim representing a scope that is requested when requesting an Access
    /// Token from the Identity Provider.
    /// </summary>
    public static Claim ScopeClaim(string scope)
    {
        return new Claim(EesClaimTypes.SupportedMsalScope, scope);
    }

    /// <summary>
    /// Create a generic named Claim.
    /// </summary>
    public static Claim GenericClaim(string claimName)
    {
        return new Claim(claimName, "");
    }
}

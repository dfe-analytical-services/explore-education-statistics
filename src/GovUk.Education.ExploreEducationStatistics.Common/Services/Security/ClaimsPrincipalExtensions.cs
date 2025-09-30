#nullable enable
using System.Security.Claims;
using GovUk.Education.ExploreEducationStatistics.Common.Extensions;

namespace GovUk.Education.ExploreEducationStatistics.Common.Services.Security;

public static class ClaimsPrincipalExtensions
{
    public static Guid GetUserId(this ClaimsPrincipal principal)
    {
        var userIdClaim = principal.FindFirstValue(EesClaimTypes.LocalUserId);
        return Guid.Parse(userIdClaim);
    }

    public static string GetEmail(this ClaimsPrincipal principal)
    {
        // Keycloak will always return email addresses in the
        // "http://schemas.xmlsoap.org/ws/2005/05/identity/claims/emailaddress" Claim.
        //
        // Entra ID on the other hand will return email addresses in either the
        // "http://schemas.xmlsoap.org/ws/2005/05/identity/claims/emailaddress" Claim or the
        // "http://schemas.xmlsoap.org/ws/2005/05/identity/claims/name" Claim, depending on its configuration.
        //
        // We need to account for either scenario so as not to rely one one Entra ID setup.
        return principal.FindFirstValue(ClaimTypes.Email) ?? principal.FindFirstValue(ClaimTypes.Name);
    }

    public static (string FirstName, string LastName) GetNameParts(this ClaimsPrincipal principal)
    {
        // Keycloak will always return first and last names in the
        // "http://schemas.xmlsoap.org/ws/2005/05/identity/claims/givenname" and
        // "http://schemas.xmlsoap.org/ws/2005/05/identity/claims/surname" Claims.
        //
        // Entra ID on the other hand will return either separated first and last names in the
        // "http://schemas.xmlsoap.org/ws/2005/05/identity/claims/givenname" and
        // "http://schemas.xmlsoap.org/ws/2005/05/identity/claims/surname" Claims,
        // or a combined, space-separated first name and last name within the "name" Claim (not
        // to be confused with the "http://schemas.xmlsoap.org/ws/2005/05/identity/claims/name" Claim
        // that it sometimes uses to return email addresses).
        //
        // We need to account for either scenario so as not to rely one one Entra ID setup.
        var givenName = principal.FindFirstValue(ClaimTypes.GivenName);
        var surname = principal.FindFirstValue(ClaimTypes.Surname);

        if (givenName != null && surname != null)
        {
            return (FirstName: givenName, LastName: surname);
        }

        var nameClaim = principal.FindFirstValue(EesClaimTypes.Name);

        if (nameClaim.IsNullOrEmpty())
        {
            return (FirstName: "", LastName: "");
        }

        var nameClaimParts = nameClaim.Trim().Split(' ');

        if (nameClaimParts.Length > 1)
        {
            return (FirstName: nameClaimParts.First(), LastName: nameClaimParts.Last());
        }

        return (FirstName: nameClaimParts.First(), LastName: "");
    }

    public static bool HasScope(this ClaimsPrincipal principal, string scope)
    {
        var scopesString = principal.FindFirstValue(EesClaimTypes.SupportedMsalScope) ??
                           principal.FindFirstValue(EesClaimTypes.SupportedMsalScope2);

        if (scopesString == null)
        {
            return false;
        }

        return scopesString.Split(' ').Contains(scope);
    }
}

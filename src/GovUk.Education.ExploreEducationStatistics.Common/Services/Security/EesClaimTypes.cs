namespace GovUk.Education.ExploreEducationStatistics.Common.Services.Security;

public static class EesClaimTypes
{
    /**
     * This is the name of the Claim that will hold a user's name.  This is equivalent to JwtClaimType.Name.
     */
    public const string Name = "name";

    /**
     * This is the name of the Claim that will hold a user's roles.  This is equivalent to JwtClaimType.Role.
     */
    public const string Role = "role";

    /**
     * This Claim will hold the logged-in user's id from within the content database.
     */
    public const string LocalUserId = "LocalId";

    /**
     * This is the name of the "scope" Claim that Keycloak provides, and that is incompatible with
     * MSAL's scope Claim names that is checks for ("scp" or "http://schemas.microsoft.com/identity/claims/scope").
     */
    public const string KeycloakScope = "scope";

    /**
     * This is the name of one of the scope Claims supported by the MSAL framework. We ensure that a user's
     * scopes are transferred to this supported Claim name. This is equivalent to "ClaimConstants.Scp".
     */
    public const string SupportedMsalScope = "scp";

    /**
     * This is the name of one of the scope Claims supported by the MSAL framework. We ensure that a user's
     * scopes are transferred to this supported Claim name. This is equivalent to "ClaimConstants.Scope".
     */
    public const string SupportedMsalScope2 = "http://schemas.microsoft.com/identity/claims/scope";
}

using System.Security.Claims;
using GovUk.Education.ExploreEducationStatistics.Admin.Models;
using GovUk.Education.ExploreEducationStatistics.Common.Extensions;
using GovUk.Education.ExploreEducationStatistics.Common.Services.Security;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Identity;
using Microsoft.Identity.Web;

namespace GovUk.Education.ExploreEducationStatistics.Admin.Services;

/// <summary>
/// This service is responsible for amending a ClaimsPrincipal that has been mostly constructed already by Identity
/// Framework based upon Claims found in an incoming JWT.
///
/// It does this prior to Identity Framework completing the ClaimsPrincipal and adding it to the HttpContext for
/// downstream middleware (e.g. Authorization filters) to pick up and user.
///
/// The aim of this transformation is to allow us to standardise where certain important information lives depending
/// on the differences between Identity Providers (<see cref="TransferUnsupportedClaims"/>) which is then used by
/// downstream middleware successfully.
///
/// Additionally, this transformation allows us to add additional information that was not included in the original
/// JWT from the Identity Provider, such as the user's roles and permissions within EES itself, and their local EES
/// User Id.  These are useful pieces of information that code in our Controllers and Services can use (
/// <see cref="AddRolesAndClaims"/> and <see cref="AddUserId"/>).
/// </summary>
public class ClaimsPrincipalTransformationService : IClaimsTransformation
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly RoleManager<IdentityRole> _roleManager;

    public ClaimsPrincipalTransformationService(
            UserManager<ApplicationUser> userManager,
            RoleManager<IdentityRole> roleManager)
        {
            _userManager = userManager;
            _roleManager = roleManager;
        }

        public async Task<ClaimsPrincipal> TransformAsync(ClaimsPrincipal principal)
        {
            var email = principal.GetEmail();

            // If we cannot identify the current user, there is no more transformation to be done.
            if (email.IsNullOrEmpty())
            {
                return principal;
            }
            
            var localIdentity = new ClaimsIdentity(
                authenticationType: "",
                nameType: EesClaimTypes.Name,
                roleType: EesClaimTypes.Role);

            principal.AddIdentity(localIdentity);

            TransferUnsupportedClaims(principal, localIdentity);

            var user = await _userManager.FindByEmailAsync(email);

            if (user == null)
            {
                return principal;
            }

            await AddRolesAndClaims(localIdentity, user);
            AddUserId(localIdentity, user);
            return principal;
        }

        /// <summary>
        /// Add the user's EES-specific User Id to the ClaimsPrincipal.
        /// </summary>
        private void AddUserId(ClaimsIdentity localIdentity, ApplicationUser user)
        {
            localIdentity.AddClaim(new Claim(EesClaimTypes.LocalUserId, user.Id));
        }

        /// <summary>
        /// Add the user's EES-specific Global, Release and Publication roles to the ClaimsPrincipal.
        /// </summary>
        private async Task AddRolesAndClaims(ClaimsIdentity localIdentity, ApplicationUser user)
        {
            var directUserClaims = await _userManager.GetClaimsAsync(user);

            // Grab the user's indirect claims via their roles.
            var roleNames = await _userManager.GetRolesAsync(user);
            var indirectRoleClaims = roleNames
                .Select(async roleName => await _roleManager.FindByNameAsync(roleName))
                .Select(role => _roleManager.GetClaimsAsync(role.Result))
                .SelectMany(claim => claim.Result);

            // Add direct and indirect claims to the ClaimsPrincipal so we can provide claims-based authorization.
            localIdentity.AddClaims(directUserClaims);
            localIdentity.AddClaims(indirectRoleClaims);

            // Add the user's global roles as role-type claims, so we can provide role-based authorization.
            var roleClaims = roleNames
                .ToList()
                .Select(roleName => new Claim(EesClaimTypes.Role, roleName));
            localIdentity.AddClaims(roleClaims);
        }

        // Attempt to transfer an unsupported scope Claim into one of the 2 supported scope Claim names that
        // Microsoft Platform Identity supports - "scp" or "http://schemas.microsoft.com/identity/claims/scope".
        //
        // This occurs when using an IdP that does not issue the Scope claim with one of the 2 supported names
        // above.  An example would be Keycloak, which issues scopes under a claim named "scope".
        //
        // This ensures that the Authorization filters further up the request-processing chain are provided with
        // a compatible scope Claim from which to enforce access to endpoints based upon scopes.
        private static void TransferUnsupportedClaims(ClaimsPrincipal principal, ClaimsIdentity localIdentity)
        {
            var supportedScopeClaim = principal.FindFirst(claim =>
                claim.Type == ClaimConstants.Scp || claim.Type == ClaimConstants.Scope);

            if (supportedScopeClaim == null)
            {
                var unsupportedScopeClaim = principal.FindFirst(claim => claim.Type == EesClaimTypes.KeycloakScope);
                if (unsupportedScopeClaim != null)
                {
                    localIdentity.AddClaim(new Claim(EesClaimTypes.SupportedMsalScope, unsupportedScopeClaim.Value));
                }
            }
        }
}

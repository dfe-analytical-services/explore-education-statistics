using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using GovUk.Education.ExploreEducationStatistics.Admin.Models;
using IdentityModel;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Identity;
using Microsoft.Identity.Web;

namespace GovUk.Education.ExploreEducationStatistics.Admin.Services;

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
            var localIdentity = new ClaimsIdentity();
            principal.AddIdentity(localIdentity);

            TransferUnsupportedClaims(principal, localIdentity);

            // TODO EES-4814 - can we just rely on a single set of Claims now?
            var email = principal.FindFirstValue(ClaimTypes.Email) ?? principal.FindFirstValue(ClaimTypes.Name);

            var user = await _userManager.FindByEmailAsync(email);

            if (user == null)
            {
                return principal;
            }

            await AddRolesAndClaims(localIdentity, user);
            AddUserId(localIdentity, user);
            return principal;
        }

        private void AddUserId(ClaimsIdentity localIdentity, ApplicationUser user)
        {
            // TODO EES-4814 - put EES-specific claims somewhere better.
            localIdentity.AddClaim(new Claim(Common.Services.Security.ClaimsPrincipalExtensions.EesUserIdClaim, user.Id));
        }

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
                .Select(roleName => new Claim(JwtClaimTypes.Role, roleName));
            localIdentity.AddClaims(roleClaims);
        }

        private static void TransferUnsupportedClaims(ClaimsPrincipal principal, ClaimsIdentity localIdentity)
        {
            // Attempt to transfer an unsupported scope Claim into one of the 2 supported scope Claim names that
            // Microsoft Platform Identity supports - "scp" or "http://schemas.microsoft.com/identity/claims/scope".
            //
            // This occurs when using an IdP that does not issue the Scope claim with one of the 2 supported names
            // above.  An example would be Keycloak, which issues scopes under a claim named "scope".
            //
            // This ensures that the Authorization filters further up the request-processing chain are provided with
            // a compatible scope Claim from which to enforce access to endpoints based upon scopes.

            var supportedScopeClaim = principal.FindFirst(claim =>
                claim.Type == ClaimConstants.Scp || claim.Type == ClaimConstants.Scope);

            if (supportedScopeClaim == null)
            {
                var unsupportedScopeClaim = principal.FindFirst(claim => claim.Type == "scope");
                if (unsupportedScopeClaim != null)
                {
                    localIdentity.AddClaim(new Claim(ClaimConstants.Scp, unsupportedScopeClaim.Value));
                }
            }

            principal.AddIdentity(localIdentity);
        }
}

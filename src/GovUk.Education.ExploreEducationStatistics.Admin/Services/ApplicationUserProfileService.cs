using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using GovUk.Education.ExploreEducationStatistics.Admin.Models;
using IdentityModel;
using IdentityServer4.Models;
using IdentityServer4.Services;
using Microsoft.AspNetCore.Identity;

namespace GovUk.Education.ExploreEducationStatistics.Admin.Services
{
    public class ApplicationUserProfileService : IProfileService
    {
        private readonly UserManager<ApplicationUser> _userManager;

        public ApplicationUserProfileService(UserManager<ApplicationUser> userManager)
        {
            _userManager = userManager;
        }

        async Task IProfileService.GetProfileDataAsync(ProfileDataRequestContext context)
        {
            // 2 different calls to this endpoint (one to generate the JWT, the other to retrieve user information
            // as per the OpenId /userinfo endpoint) provide different ID claims for which we need to look up our user
            var userId = context.Subject.FindFirst(claim => 
                claim.Type == JwtClaimTypes.Subject || 
                claim.Type == ClaimTypes.NameIdentifier).Value;
            
            var user = await _userManager.FindByIdAsync(userId);
            var roles = await _userManager.GetRolesAsync(user);
            var roleClaims = roles.Select(role => new Claim(JwtClaimTypes.Role, role));
            
            // add role and profile info to JWT claims
            context.IssuedClaims.AddRange(roleClaims);
            context.IssuedClaims.Add(new Claim(JwtClaimTypes.GivenName, user.FirstName));
            context.IssuedClaims.Add(new Claim(JwtClaimTypes.FamilyName, user.LastName));
        }

        public Task IsActiveAsync(IsActiveContext context)
        {
            return Task.CompletedTask;
        }
    }
}
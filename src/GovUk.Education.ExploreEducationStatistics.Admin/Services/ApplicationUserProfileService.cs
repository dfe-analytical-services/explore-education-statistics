using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using GovUk.Education.ExploreEducationStatistics.Admin.Models;
using GovUk.Education.ExploreEducationStatistics.Admin.Security;
using GovUk.Education.ExploreEducationStatistics.Content.Model.Database;
using IdentityModel;
using IdentityServer4.Models;
using IdentityServer4.Services;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace GovUk.Education.ExploreEducationStatistics.Admin.Services
{
    public class ApplicationUserProfileService : IProfileService
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly ContentDbContext _contentDbContext;
            
        public ApplicationUserProfileService(
            UserManager<ApplicationUser> userManager, 
            RoleManager<IdentityRole> roleManager, 
            ContentDbContext contentDbContext)
        {
            _userManager = userManager;
            _roleManager = roleManager;
            _contentDbContext = contentDbContext;
        }

        async Task IProfileService.GetProfileDataAsync(ProfileDataRequestContext context)
        {
            // 2 different calls to this endpoint (one to generate the JWT, the other to retrieve user information
            // as per the OpenId /userinfo endpoint) provide different ID claims for which we need to look up our user
            var userId = context.Subject.FindFirst(claim => 
                claim.Type == JwtClaimTypes.Subject || 
                claim.Type == ClaimTypes.NameIdentifier).Value;
            
            // Grab the user's direct Claims
            var user = await _userManager.FindByIdAsync(userId);
            var userClaims = await _userManager.GetClaimsAsync(user);
            
            // Grab the user's Claims via Roles
            var roleNames = await _userManager.GetRolesAsync(user);
            var roleClaims = roleNames
                .Select(async roleName => await _roleManager.FindByNameAsync(roleName))
                .Select(role => _roleManager.GetClaimsAsync(role.Result))
                .SelectMany(claim => claim.Result);
            
            // Add user claims, role claims and profile information to JWT claims
            context.IssuedClaims.AddRange(userClaims);
            context.IssuedClaims.AddRange(roleClaims);
            context.IssuedClaims.Add(new Claim(JwtClaimTypes.GivenName, user.FirstName));
            context.IssuedClaims.Add(new Claim(JwtClaimTypes.FamilyName, user.LastName));
            
            // Add the user's Roles, so we can optionally provide role-based authorization
            roleNames.ToList().ForEach(roleName => 
                context.IssuedClaims.Add(new Claim(JwtClaimTypes.Role, roleName)));

            var internalUser = await _contentDbContext
                .Users
                .SingleAsync(u => u.Email.ToLower() == user.Email.ToLower());
            
            // Add the service's User Id to the JWT so that it can be looked up later when determining which service
            // User is performing actions
            context.IssuedClaims.Add(new Claim(UserClaimTypes.InternalUserId.ToString(), internalUser.Id.ToString()));
        }

        public Task IsActiveAsync(IsActiveContext context)
        {
            return Task.CompletedTask;
        }
    }
}
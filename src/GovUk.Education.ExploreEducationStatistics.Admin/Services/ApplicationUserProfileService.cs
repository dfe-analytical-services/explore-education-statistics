using System.Threading.Tasks;
using IdentityModel;
using IdentityServer4.Models;
using IdentityServer4.Services;

namespace GovUk.Education.ExploreEducationStatistics.Admin.Services
{
    public class ApplicationUserProfileService : IProfileService
    {
        Task IProfileService.GetProfileDataAsync(ProfileDataRequestContext context)
        {
            // get role claims from ClaimsPrincipal 
            var roleClaims = context.Subject.FindAll(JwtClaimTypes.Role);

            // add role claims to the generated JWT
            context.IssuedClaims.AddRange(roleClaims);
            return Task.CompletedTask;
        }

        public Task IsActiveAsync(IsActiveContext context)
        {
            return Task.CompletedTask;
        }
    }
}
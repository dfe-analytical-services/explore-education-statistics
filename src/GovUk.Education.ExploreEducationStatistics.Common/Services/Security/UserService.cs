#nullable enable
using System;
using System.Security.Claims;
using System.Threading.Tasks;
using GovUk.Education.ExploreEducationStatistics.Common.Services.Interfaces.Security;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;

namespace GovUk.Education.ExploreEducationStatistics.Common.Services.Security
{
    public class UserService : IUserService
    {
        private readonly IAuthorizationService _authorizationService;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public UserService(IAuthorizationService authorizationService, IHttpContextAccessor httpContextAccessor)
        {
            _authorizationService = authorizationService;
            _httpContextAccessor = httpContextAccessor;
        }

        public Task<bool> MatchesPolicy(Enum policy)
        {
            return _authorizationService.MatchesPolicy(GetUser(), policy);
        }

        public Task<bool> MatchesPolicy(object resource, Enum policy)
        {
            return _authorizationService.MatchesPolicy(GetUser(), resource, policy);
        }

        public Guid GetUserId()
        {
            return GetUser().GetUserId();
        }

        public UserProfile GetProfile()
        {
            var user = GetUser();

            // TODO EES-4814 - can we just rely on a single set of Claims now?
            var email = user.FindFirstValue(ClaimTypes.Email) ?? user.FindFirstValue(ClaimTypes.Name);

            // Try to infer the user's name from the explicit "Given Name" and "Surname" Claims
            // if they are available.
            // TODO EES-4814 - can we just rely on a single set of Claims now?
            var givenName = user.FindFirstValue(ClaimTypes.GivenName);
            var surname = user.FindFirstValue(ClaimTypes.Surname);

            return new UserProfile(Email: email, FirstName: givenName, LastName: surname);
        }

        private ClaimsPrincipal? GetUser()
        {
            return _httpContextAccessor.HttpContext?.User;
        }
    }
}

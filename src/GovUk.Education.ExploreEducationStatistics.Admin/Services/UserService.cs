using System;
using System.Security.Claims;
using System.Threading.Tasks;
using GovUk.Education.ExploreEducationStatistics.Admin.Security;
using GovUk.Education.ExploreEducationStatistics.Admin.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;

namespace GovUk.Education.ExploreEducationStatistics.Admin.Services
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

        public Task<bool> MatchesPolicy(SecurityPolicies policy)
        {
            return _authorizationService.MatchesPolicy(GetUser(), policy);
        }

        public Task<bool> MatchesPolicy(object resource, SecurityPolicies policy)
        {
            return _authorizationService.MatchesPolicy(GetUser(), resource, policy);
        }

        public Guid GetUserId()
        {
            return SecurityUtils.GetUserId(GetUser());
        }

        private ClaimsPrincipal GetUser()
        {
            return _httpContextAccessor.HttpContext.User;
        }
    }
}
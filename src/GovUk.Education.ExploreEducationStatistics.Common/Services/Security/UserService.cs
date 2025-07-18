#nullable enable
using System;
using System.Security.Authentication;
using System.Security.Claims;
using System.Threading.Tasks;
using GovUk.Education.ExploreEducationStatistics.Common.Services.Interfaces.Security;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;

namespace GovUk.Education.ExploreEducationStatistics.Common.Services.Security;

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
        var user = GetUser();

        if (user == null)
        {
            throw new AuthenticationException("No user was found to get Id from");
        }

        return user.GetUserId();
    }

    public UserProfileFromClaims GetProfileFromClaims()
    {
        var user = GetUser();

        if (user == null)
        {
            throw new AuthenticationException("No user was found to get Claims from");
        }

        var email = user.GetEmail();
        var (firstName, lastName) = user.GetNameParts();
        return new UserProfileFromClaims(Email: email, FirstName: firstName, LastName: lastName);
    }

    private ClaimsPrincipal? GetUser()
    {
        return _httpContextAccessor.HttpContext?.User;
    }
}

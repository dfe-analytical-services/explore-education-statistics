#nullable enable
using System;
using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;

namespace GovUk.Education.ExploreEducationStatistics.Common.Security;

public static class AuthorizationHandlerContextFactory
{
    public static AuthorizationHandlerContext CreateAnonymousAuthContext<TRequirement, TResource>(TResource resource)
        where TRequirement : IAuthorizationRequirement
    {
        var requirements = new IAuthorizationRequirement[] {Activator.CreateInstance<TRequirement>()};
        ClaimsPrincipal user = null!;
        return new AuthorizationHandlerContext(requirements, user, resource);
    }

    public static AuthorizationHandlerContext CreateAuthContext<TRequirement, TResource>(ClaimsPrincipal user,
        TResource resource) where TRequirement : IAuthorizationRequirement
    {
        var requirements = new IAuthorizationRequirement[] {Activator.CreateInstance<TRequirement>()};
        return new AuthorizationHandlerContext(requirements, user, resource);
    }

    public static AuthorizationHandlerContext CreateAuthContext<TRequirement>(ClaimsPrincipal user,
        object? resource) where TRequirement : IAuthorizationRequirement
    {
        var requirements = new IAuthorizationRequirement[] {Activator.CreateInstance<TRequirement>()};
        return new AuthorizationHandlerContext(requirements, user, resource);
    }
}

#nullable enable
using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.SignalR;

namespace GovUk.Education.ExploreEducationStatistics.Admin.Hubs.Filters;

/// <summary>
/// Sets <see cref="HttpContext"/> in <see cref="IHttpContextAccessor"/> allowing
/// access to it across the execution context e.g. in services outside of SignalR.
/// </summary>
public class HttpContextHubFilter : IHubFilter
{
    private readonly IHttpContextAccessor _httpContextAccessor;

    public HttpContextHubFilter(IHttpContextAccessor httpContextAccessor)
    {
        _httpContextAccessor = httpContextAccessor;
    }

    public async ValueTask<object?> InvokeMethodAsync(
        HubInvocationContext invocationContext,
        Func<HubInvocationContext, ValueTask<object?>> next)
    {
        var contextSet = false;
        
        try
        {
            if (_httpContextAccessor.HttpContext is null)
            {
                _httpContextAccessor.HttpContext = invocationContext.Context.GetHttpContext();
                contextSet = true;
            }
        
            return await next(invocationContext);
        }
        finally
        {
            if (contextSet)
            {
                _httpContextAccessor.HttpContext = null;
            }
        }
    }
}
using GovUk.Education.ExploreEducationStatistics.Public.Data.Api.Security;
using GovUk.Education.ExploreEducationStatistics.Public.Data.Api.Services.Interfaces.Security;

namespace GovUk.Education.ExploreEducationStatistics.Public.Data.Api.Services.Security;

public class AuthorizationHandlerService(
    IHttpContextAccessor httpContextAccessor,
    IWebHostEnvironment environment)
    : IAuthorizationHandlerService
{
    public bool CanAccessUnpublishedData()
    {
        // Simulate authentication in non-Azure environments.
        if (!environment.IsProduction() &&
            httpContextAccessor.HttpContext?.Request.Headers.UserAgent.Contains(
                Common.Security.SecurityConstants.AdminUserAgent) == true)
        {
            return true;
        }
        
        var user = httpContextAccessor.HttpContext?.User;

        if (user is not null && user.IsInRole(SecurityConstants.AdminAccessAppRole))
        {
            return true;
        }

        return false;
    }
}

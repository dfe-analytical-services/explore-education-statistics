using GovUk.Education.ExploreEducationStatistics.Public.Data.Api.Security;
using GovUk.Education.ExploreEducationStatistics.Public.Data.Api.Services.Interfaces.Security;

namespace GovUk.Education.ExploreEducationStatistics.Public.Data.Api.Services.Security;

public class AuthorizationService(
    IHttpContextAccessor httpContextAccessor,
    IWebHostEnvironment environment)
    : IAuthorizationService
{
    public bool CanAccessUnpublishedData()
    {
        // Simulate authentication in non-Azure environments.
        if (!environment.IsProduction() &&
            httpContextAccessor.HttpContext?.Request.Headers.UserAgent.Contains(SecurityConstants.AdminUserAgent) == true)
        {
            return true;
        }
        
        var user = httpContextAccessor.HttpContext?.User;

        if (user is not null && user.IsInRole(SecurityConstants.UnpublishedDataReaderAppRole))
        {
            return true;
        }

        return false;
    }
}

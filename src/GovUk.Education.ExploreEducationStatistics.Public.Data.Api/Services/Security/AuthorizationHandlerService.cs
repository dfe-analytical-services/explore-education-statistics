using GovUk.Education.ExploreEducationStatistics.Common.Extensions;
using GovUk.Education.ExploreEducationStatistics.Public.Data.Api.Constants;
using GovUk.Education.ExploreEducationStatistics.Public.Data.Api.Security;
using GovUk.Education.ExploreEducationStatistics.Public.Data.Api.Services.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Public.Data.Api.Services.Interfaces.Security;
using GovUk.Education.ExploreEducationStatistics.Public.Data.Model;
using Microsoft.Extensions.Primitives;

namespace GovUk.Education.ExploreEducationStatistics.Public.Data.Api.Services.Security;

public class AuthorizationHandlerService(
    IHttpContextAccessor httpContextAccessor,
    IWebHostEnvironment environment,
    IPreviewTokenService previewTokenService)
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
    
    public async Task<bool> RequestHasValidPreviewToken(DataSet dataSet)
    {
        return TryGetPreviewToken(out var previewToken) && 
               await previewTokenService.ValidatePreviewTokenForDataSet(
                   previewToken: previewToken.ToString(),
                   dataSetId: dataSet.Id);
    }

    public async Task<bool> RequestHasValidPreviewToken(DataSetVersion dataSetVersion)
    {
        return TryGetPreviewToken(out var previewToken) && 
               await previewTokenService.ValidatePreviewTokenForDataSetVersion(
                   previewToken: previewToken.ToString(),
                   dataSetVersionId: dataSetVersion.Id);
    }

    private bool TryGetPreviewToken(out StringValues previewToken)
    {
        return httpContextAccessor.HttpContext.TryGetRequestHeader(
            RequestHeaderNames.PreviewToken,
            out previewToken);       
    }
}

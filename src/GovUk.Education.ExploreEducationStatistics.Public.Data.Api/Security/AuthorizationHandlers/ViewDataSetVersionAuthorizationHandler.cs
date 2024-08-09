using GovUk.Education.ExploreEducationStatistics.Common.Extensions;
using GovUk.Education.ExploreEducationStatistics.Public.Data.Api.Services.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Public.Data.Model;
using Microsoft.AspNetCore.Authorization;

namespace GovUk.Education.ExploreEducationStatistics.Public.Data.Api.Security.AuthorizationHandlers;

public class ViewDataSetVersionRequirement : IAuthorizationRequirement;

public class ViewDataSetVersionAuthorizationHandler(
    IHttpContextAccessor httpContextAccessor,
    IPreviewTokenService previewTokenService)
    : AuthorizationHandler<ViewDataSetVersionRequirement, DataSetVersion>
{
    protected override async Task HandleRequirementAsync(
        AuthorizationHandlerContext context,
        ViewDataSetVersionRequirement requirement,
        DataSetVersion dataSetVersion)
    {
        // TODO: EES-5374 - Temporary workaround until authentication is added for admin API
        if (httpContextAccessor.HttpContext?.Request.Headers.UserAgent.Contains("EES Admin") == true)
        {
            context.Succeed(requirement);
            return;
        }

        if (dataSetVersion.Status is DataSetVersionStatus.Published
            or DataSetVersionStatus.Deprecated
            or DataSetVersionStatus.Withdrawn)
        {
            context.Succeed(requirement);
            return;
        }

        if (await RequestHasValidPreviewToken(dataSetVersion))
        {
            context.Succeed(requirement);
        }
    }

    private async Task<bool> RequestHasValidPreviewToken(DataSetVersion dataSetVersion)
    {
        return httpContextAccessor.HttpContext.TryGetRequestHeader(
                   RequestHeaderConstants.PreviewTokenRequestHeaderName,
                   out var previewToken)
               && await previewTokenService.ValidatePreviewToken(previewToken, dataSetVersion.Id);
    }
}

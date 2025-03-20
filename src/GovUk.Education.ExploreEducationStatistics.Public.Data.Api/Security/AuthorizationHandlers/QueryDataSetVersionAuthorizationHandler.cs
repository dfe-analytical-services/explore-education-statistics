using GovUk.Education.ExploreEducationStatistics.Common.Extensions;
using GovUk.Education.ExploreEducationStatistics.Public.Data.Api.Constants;
using GovUk.Education.ExploreEducationStatistics.Public.Data.Api.Services.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Public.Data.Model;
using Microsoft.AspNetCore.Authorization;

namespace GovUk.Education.ExploreEducationStatistics.Public.Data.Api.Security.AuthorizationHandlers;

public class QueryDataSetVersionRequirement : IAuthorizationRequirement;

public class QueryDataSetVersionAuthorizationHandler(
    IHttpContextAccessor httpContextAccessor,
    IPreviewTokenService previewTokenService)
    : AuthorizationHandler<QueryDataSetVersionRequirement, DataSetVersion>
{
    protected override async Task HandleRequirementAsync(
        AuthorizationHandlerContext context,
        QueryDataSetVersionRequirement requirement,
        DataSetVersion dataSetVersion)
    {
        if (dataSetVersion.Status is DataSetVersionStatus.Published or DataSetVersionStatus.Deprecated)
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
                   RequestHeaderNames.PreviewToken,
                   out var previewToken)
               && await previewTokenService.ValidatePreviewTokenForDataSetVersion(previewToken.ToString(), dataSetVersion.Id);
    }
}

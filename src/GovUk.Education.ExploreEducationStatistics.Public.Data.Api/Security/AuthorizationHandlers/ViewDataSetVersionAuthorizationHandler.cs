using GovUk.Education.ExploreEducationStatistics.Public.Data.Model;
using Microsoft.AspNetCore.Authorization;

namespace GovUk.Education.ExploreEducationStatistics.Public.Data.Api.Security.AuthorizationHandlers;

public class ViewDataSetVersionRequirement : IAuthorizationRequirement;

public class ViewDataSetVersionAuthorizationHandler(
    IHttpContextAccessor httpContextAccessor)
    : AuthorizationHandler<ViewDataSetVersionRequirement, DataSetVersion>
{
    protected override Task HandleRequirementAsync(
        AuthorizationHandlerContext context,
        ViewDataSetVersionRequirement requirement,
        DataSetVersion dataSetVersion)
    {
        // TODO: EES-5374 - Temporary workaround until authentication is added for admin API
        if (httpContextAccessor.HttpContext?.Request.Headers.UserAgent.Contains("EES Admin") == true)
        {
            context.Succeed(requirement);
            return Task.CompletedTask;
        }

        if (dataSetVersion.Status is DataSetVersionStatus.Published
            or DataSetVersionStatus.Deprecated
            or DataSetVersionStatus.Withdrawn)
        {
            context.Succeed(requirement);
        }

        return Task.CompletedTask;
    }
}

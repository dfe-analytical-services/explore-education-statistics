using GovUk.Education.ExploreEducationStatistics.Common.Services.Security;
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
        var user = httpContextAccessor.HttpContext?.User;
        
        if (user is not null && user.HasScope(SecurityScopes.AdminApiAccess))
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

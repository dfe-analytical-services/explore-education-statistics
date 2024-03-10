using GovUk.Education.ExploreEducationStatistics.Public.Data.Model;
using Microsoft.AspNetCore.Authorization;

namespace GovUk.Education.ExploreEducationStatistics.Public.Data.Api.Security.AuthorizationHandlers;

public class ViewDataSetVersionRequirement : IAuthorizationRequirement;

public class ViewDataSetVersionAuthorizationHandler 
    : AuthorizationHandler<ViewDataSetVersionRequirement, DataSetVersion>
{
    protected override Task HandleRequirementAsync(
        AuthorizationHandlerContext context,
        ViewDataSetVersionRequirement requirement,
        DataSetVersion dataSetVersion)
    {
        if (dataSetVersion.Status is DataSetVersionStatus.Published
            or DataSetVersionStatus.Deprecated
            or DataSetVersionStatus.Withdrawn)
        {
            context.Succeed(requirement);
        }

        return Task.CompletedTask;
    }
}

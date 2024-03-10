using GovUk.Education.ExploreEducationStatistics.Public.Data.Model;
using Microsoft.AspNetCore.Authorization;

namespace GovUk.Education.ExploreEducationStatistics.Public.Data.Api.Security.AuthorizationHandlers;

public class QueryDataSetVersionRequirement : IAuthorizationRequirement;

public class QueryDataSetVersionAuthorizationHandler
    : AuthorizationHandler<QueryDataSetVersionRequirement, DataSetVersion>
{
    protected override Task HandleRequirementAsync(
        AuthorizationHandlerContext context,
        QueryDataSetVersionRequirement requirement,
        DataSetVersion dataSetVersion)
    {
        if (dataSetVersion.Status is DataSetVersionStatus.Published or DataSetVersionStatus.Deprecated)
        {
            context.Succeed(requirement);
        }

        return Task.CompletedTask;
    }
}

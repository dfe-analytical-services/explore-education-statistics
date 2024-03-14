using GovUk.Education.ExploreEducationStatistics.Public.Data.Model;
using Microsoft.AspNetCore.Authorization;

namespace GovUk.Education.ExploreEducationStatistics.Public.Data.Api.Security.AuthorizationHandlers;

public class ViewDataSetRequirement : IAuthorizationRequirement;

public class ViewDataSetAuthorizationHandler : AuthorizationHandler<ViewDataSetRequirement, DataSet>
{
    protected override Task HandleRequirementAsync(
        AuthorizationHandlerContext context,
        ViewDataSetRequirement requirement,
        DataSet dataSet)
    {
        if (dataSet.Status is DataSetStatus.Published or DataSetStatus.Deprecated)
        {
            context.Succeed(requirement);
        }

        return Task.CompletedTask;
    }
}

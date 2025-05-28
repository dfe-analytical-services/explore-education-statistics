using GovUk.Education.ExploreEducationStatistics.Public.Data.Api.Services.Interfaces.Security;
using GovUk.Education.ExploreEducationStatistics.Public.Data.Model;
using Microsoft.AspNetCore.Authorization;
using IAuthorizationService =
    GovUk.Education.ExploreEducationStatistics.Public.Data.Api.Services.Interfaces.Security.IAuthorizationService;

namespace GovUk.Education.ExploreEducationStatistics.Public.Data.Api.Security.AuthorizationHandlers;

public class ViewDataSetRequirement : IAuthorizationRequirement;

public class ViewDataSetAuthorizationHandler(IAuthorizationService authorizationService)
    : AuthorizationHandler<ViewDataSetRequirement, DataSet>
{
    protected override async Task HandleRequirementAsync(
        AuthorizationHandlerContext context,
        ViewDataSetRequirement requirement,
        DataSet dataSet)
    {
        if (dataSet.Status is DataSetStatus.Published or DataSetStatus.Deprecated)
        {
            context.Succeed(requirement);
            return;
        }

        if (await authorizationService.RequestHasValidPreviewToken(dataSet))
        {
            context.Succeed(requirement);
        }
    }
}

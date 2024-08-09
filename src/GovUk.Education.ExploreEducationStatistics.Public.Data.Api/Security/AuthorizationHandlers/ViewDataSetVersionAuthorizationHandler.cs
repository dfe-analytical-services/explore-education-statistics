using GovUk.Education.ExploreEducationStatistics.Public.Data.Model;
using Microsoft.AspNetCore.Authorization;
using IAuthorizationService =
    GovUk.Education.ExploreEducationStatistics.Public.Data.Api.Services.Interfaces.Security.IAuthorizationService;

namespace GovUk.Education.ExploreEducationStatistics.Public.Data.Api.Security.AuthorizationHandlers;

public class ViewDataSetVersionRequirement : IAuthorizationRequirement;

public class ViewDataSetVersionAuthorizationHandler(
    IAuthorizationService authorizationService)
    : AuthorizationHandler<ViewDataSetVersionRequirement, DataSetVersion>
{
    protected override Task HandleRequirementAsync(
        AuthorizationHandlerContext context,
        ViewDataSetVersionRequirement requirement,
        DataSetVersion dataSetVersion)
    {
        if (authorizationService.CanAccessUnpublishedData())
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

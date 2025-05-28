using GovUk.Education.ExploreEducationStatistics.Public.Data.Api.Services.Interfaces.Security;
using GovUk.Education.ExploreEducationStatistics.Public.Data.Model;
using GovUk.Education.ExploreEducationStatistics.Public.Data.Model.Extensions;
using Microsoft.AspNetCore.Authorization;
using IAuthorizationService =
    GovUk.Education.ExploreEducationStatistics.Public.Data.Api.Services.Interfaces.Security.IAuthorizationService;

namespace GovUk.Education.ExploreEducationStatistics.Public.Data.Api.Security.AuthorizationHandlers;

public class ViewDataSetVersionRequirement : IAuthorizationRequirement;

public class ViewDataSetVersionAuthorizationHandler(IAuthorizationService authorizationService)
    : AuthorizationHandler<ViewDataSetVersionRequirement, DataSetVersion>
{
    protected override async Task HandleRequirementAsync(
        AuthorizationHandlerContext context,
        ViewDataSetVersionRequirement requirement,
        DataSetVersion dataSetVersion)
    {
        if (authorizationService.CanAccessUnpublishedData())
        {
            context.Succeed(requirement);
            return;
        }

        if (dataSetVersion.IsPublicStatus())
        {
            context.Succeed(requirement);
            return;
        }

        if (await authorizationService.RequestHasValidPreviewToken(dataSetVersion))
        {
            context.Succeed(requirement);
        }
    }
}

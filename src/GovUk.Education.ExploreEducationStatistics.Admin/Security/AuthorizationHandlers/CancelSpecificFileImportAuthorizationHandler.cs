#nullable enable
using GovUk.Education.ExploreEducationStatistics.Content.Model.Repository.Interfaces;
using Microsoft.AspNetCore.Authorization;
using DataImportStatusExtensions = GovUk.Education.ExploreEducationStatistics.Content.Model.DataImportStatusExtensions;
using File = GovUk.Education.ExploreEducationStatistics.Content.Model.File;

namespace GovUk.Education.ExploreEducationStatistics.Admin.Security.AuthorizationHandlers;

public class CancelSpecificFileImportRequirement : IAuthorizationRequirement { }

public class CancelSpecificFileImportAuthorizationHandler(IDataImportRepository dataImportRepository)
    : AuthorizationHandler<CancelSpecificFileImportRequirement, File>
{
    protected override async Task HandleRequirementAsync(
        AuthorizationHandlerContext context,
        CancelSpecificFileImportRequirement requirement,
        File file
    )
    {
        var status = await dataImportRepository.GetStatusByFileId(file.Id);

        if (DataImportStatusExtensions.IsFinishedOrAborting(status))
        {
            return;
        }

        if (SecurityUtils.HasClaim(context.User, SecurityClaimTypes.CancelAllFileImports))
        {
            context.Succeed(requirement);
        }
    }
}

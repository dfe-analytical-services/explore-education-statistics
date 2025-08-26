#nullable enable
using GovUk.Education.ExploreEducationStatistics.Content.Model.Repository.Interfaces;
using Microsoft.AspNetCore.Authorization;
using static GovUk.Education.ExploreEducationStatistics.Admin.Security.SecurityClaimTypes;
using DataImportStatusExtensions = GovUk.Education.ExploreEducationStatistics.Content.Model.DataImportStatusExtensions;
using File = GovUk.Education.ExploreEducationStatistics.Content.Model.File;

namespace GovUk.Education.ExploreEducationStatistics.Admin.Security.AuthorizationHandlers;

public class CancelSpecificFileImportRequirement : IAuthorizationRequirement
{
}

public class CancelSpecificFileImportAuthorizationHandler
    : AuthorizationHandler<CancelSpecificFileImportRequirement, File>
{
    private readonly IDataImportRepository _dataImportRepository;

    public CancelSpecificFileImportAuthorizationHandler(IDataImportRepository dataImportRepository)
    {
        _dataImportRepository = dataImportRepository;
    }

    protected override async Task HandleRequirementAsync(AuthorizationHandlerContext ctx,
        CancelSpecificFileImportRequirement requirement,
        File file)
    {
        var status = await _dataImportRepository.GetStatusByFileId(file.Id);

        if (DataImportStatusExtensions.IsFinishedOrAborting(status))
        {
            return;
        }

        if (SecurityUtils.HasClaim(ctx.User, CancelAllFileImports))
        {
            ctx.Succeed(requirement);
        }
    }
}

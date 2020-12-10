using System.Threading.Tasks;
using GovUk.Education.ExploreEducationStatistics.Common.Services.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Content.Model;
using Microsoft.AspNetCore.Authorization;

namespace GovUk.Education.ExploreEducationStatistics.Admin.Security.AuthorizationHandlers
{
    public class CancelSpecificFileImportRequirement : IAuthorizationRequirement
    {
    }

    public class CancelSpecificFileImportAuthorizationHandler : AuthorizationHandler<CancelSpecificFileImportRequirement, ReleaseFileReference>
    {
        private readonly IImportStatusService _importStatusService;

        public CancelSpecificFileImportAuthorizationHandler(IImportStatusService importStatusService)
        {
            _importStatusService = importStatusService;
        }

        protected override async Task HandleRequirementAsync(AuthorizationHandlerContext ctx,
            CancelSpecificFileImportRequirement requirement,
            ReleaseFileReference dataFile)
        {
            if (await _importStatusService.IsImportFinished(dataFile.ReleaseId, dataFile.Filename))
            {
                return;
            }

            if (SecurityUtils.HasClaim(ctx.User, SecurityClaimTypes.CancelAllFileImports))
            {
                ctx.Succeed(requirement);
            }
        }
    }
}
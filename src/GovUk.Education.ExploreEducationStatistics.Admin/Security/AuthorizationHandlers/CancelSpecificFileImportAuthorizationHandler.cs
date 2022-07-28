#nullable enable
using System.Threading.Tasks;
using GovUk.Education.ExploreEducationStatistics.Content.Model;
using GovUk.Education.ExploreEducationStatistics.Content.Model.Repository.Interfaces;
using Microsoft.AspNetCore.Authorization;
using static GovUk.Education.ExploreEducationStatistics.Admin.Security.SecurityClaimTypes;

namespace GovUk.Education.ExploreEducationStatistics.Admin.Security.AuthorizationHandlers
{
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

            if (status.IsFinishedOrAborting())
            {
                return;
            }

            if (SecurityUtils.HasClaim(ctx.User, CancelAllFileImports))
            {
                ctx.Succeed(requirement);
            }
        }
    }
}

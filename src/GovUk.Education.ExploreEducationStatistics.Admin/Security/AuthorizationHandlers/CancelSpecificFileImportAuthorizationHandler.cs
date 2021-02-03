using System.Threading.Tasks;
using GovUk.Education.ExploreEducationStatistics.Admin.Services.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Content.Model;
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
        private readonly IImportRepository _importRepository;

        public CancelSpecificFileImportAuthorizationHandler(IImportRepository importRepository)
        {
            _importRepository = importRepository;
        }

        protected override async Task HandleRequirementAsync(AuthorizationHandlerContext ctx,
            CancelSpecificFileImportRequirement requirement,
            File file)
        {
            var status = await _importRepository.GetStatusByFileId(file.Id);
            
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
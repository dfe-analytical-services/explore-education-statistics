﻿using System.Threading.Tasks;
using GovUk.Education.ExploreEducationStatistics.Admin.Models;
using GovUk.Education.ExploreEducationStatistics.Common.Model;
using GovUk.Education.ExploreEducationStatistics.Common.Services.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Content.Model;
using Microsoft.AspNetCore.Authorization;

namespace GovUk.Education.ExploreEducationStatistics.Admin.Security.AuthorizationHandlers
{
    public class CancelSpecificFileImportRequirement : IAuthorizationRequirement
    {
    }

    public class CancelSpecificFileImportAuthorizationHandler 
        : AuthorizationHandler<CancelSpecificFileImportRequirement, ReleaseFileImportInfo>
    {
        private readonly IImportStatusService _importStatusService;

        public CancelSpecificFileImportAuthorizationHandler(IImportStatusService importStatusService)
        {
            _importStatusService = importStatusService;
        }

        protected override async Task HandleRequirementAsync(AuthorizationHandlerContext ctx,
            CancelSpecificFileImportRequirement requirement,
            ReleaseFileImportInfo import)
        {
            var status = await _importStatusService.GetImportStatus(import.ReleaseId, import.DataFileName);
            
            if (status.IsFinishedOrAborting())
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
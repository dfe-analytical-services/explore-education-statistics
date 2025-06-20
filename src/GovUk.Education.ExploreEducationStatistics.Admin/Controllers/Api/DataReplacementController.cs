using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using GovUk.Education.ExploreEducationStatistics.Admin.Services.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Admin.ViewModels;
using GovUk.Education.ExploreEducationStatistics.Common.Extensions;
using GovUk.Education.ExploreEducationStatistics.Common.Model;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace GovUk.Education.ExploreEducationStatistics.Admin.Controllers.Api
{
    [Route("api")]
    [Authorize]
    [ApiController]
    public class DataReplacementController : ControllerBase
    {
        private readonly IReplacementService _replacementService;

        public DataReplacementController(IReplacementService replacementService)
        {
            _replacementService = replacementService;
        }

        [HttpGet("releases/{releaseVersionId:guid}/data/{fileId:guid}/replacement-plan/{replacementFileId:guid}")]
        public async Task<ActionResult<DataReplacementPlanViewModel>> GetReplacementPlan(Guid releaseVersionId,
            Guid fileId,
            Guid replacementFileId,
            CancellationToken cancellationToken = default)
        {
            return await _replacementService.GetReplacementPlan(
                    releaseVersionId: releaseVersionId,
                    originalFileId: fileId,
                    replacementFileId: replacementFileId,
                    cancellationToken: cancellationToken
                )
                .OnSuccess(plan => plan.ToSummary())
                .HandleFailuresOrOk();
        }

        // @MarkFix Before the frontend calls this endpoint, it calls the above replacement-plan endpoint - why?
        // @MarkFix and the frontend refetches all the data for the other files that have been imported before doing the replacement?
        // @MarkFix how does the frontend handle large files - does Data and files page properly show that it is still in progress?
        [HttpPost("releases/{releaseVersionId:guid}/data/replacements")]
        public async Task<ActionResult<Unit>> Replace(Guid releaseVersionId,
            [FromBody] List<Guid> replacementFileIds)
        {
            return await _replacementService.Replace(
                    releaseVersionId: releaseVersionId,
                    replacementFileIds: replacementFileIds
                )
                .HandleFailuresOrOk();
        }
    }
}

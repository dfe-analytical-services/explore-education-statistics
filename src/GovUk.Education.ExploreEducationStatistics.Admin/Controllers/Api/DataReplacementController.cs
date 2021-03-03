using System;
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

        [HttpGet("releases/{releaseId}/data/{fileId}/replacement-plan/{replacementFileId}")]
        public async Task<ActionResult<DataReplacementPlanViewModel>> GetReplacementPlan(Guid releaseId,
            Guid fileId,
            Guid replacementFileId)
        {
            return await _replacementService.GetReplacementPlan(
                    releaseId: releaseId,
                    originalFileId: fileId,
                    replacementFileId: replacementFileId
                )
                .OnSuccess(plan => plan.ToSummary())
                .HandleFailuresOrOk();
        }

        [HttpPost("releases/{releaseId}/data/{fileId}/replacement/{replacementFileId}")]
        public async Task<ActionResult<Unit>> Replace(Guid releaseId,
            Guid fileId,
            Guid replacementFileId)
        {
            return await _replacementService.Replace(
                    releaseId: releaseId,
                    originalFileId: fileId,
                    replacementFileId: replacementFileId
                )
                .HandleFailuresOrOk();
        }
    }
}

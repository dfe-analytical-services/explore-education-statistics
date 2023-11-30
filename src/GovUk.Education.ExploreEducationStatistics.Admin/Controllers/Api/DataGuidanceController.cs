#nullable enable
using System;
using System.Threading.Tasks;
using GovUk.Education.ExploreEducationStatistics.Admin.Requests;
using GovUk.Education.ExploreEducationStatistics.Admin.Services.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Admin.ViewModels;
using GovUk.Education.ExploreEducationStatistics.Common.Extensions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace GovUk.Education.ExploreEducationStatistics.Admin.Controllers.Api
{
    [Route("api")]
    [Authorize]
    [ApiController]
    public class DataGuidanceController : ControllerBase
    {
        private readonly IDataGuidanceService _dataGuidanceService;

        public DataGuidanceController(IDataGuidanceService dataGuidanceService)
        {
            _dataGuidanceService = dataGuidanceService;
        }

        [HttpGet("release/{releaseId:guid}/data-guidance")]
        public async Task<ActionResult<DataGuidanceViewModel>> GetReleaseDataGuidance(Guid releaseId)
        {
            return await _dataGuidanceService.GetDataGuidance(releaseId)
                .HandleFailuresOrOk();
        }

        [HttpPatch("release/{releaseId:guid}/data-guidance")]
        public async Task<ActionResult<DataGuidanceViewModel>> UpdateReleaseDataGuidance(Guid releaseId,
            DataGuidanceUpdateRequest request)
        {
            return await _dataGuidanceService.UpdateDataGuidance(releaseId, request)
                .HandleFailuresOrOk();
        }
    }
}

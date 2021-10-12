using System;
using System.Threading.Tasks;
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

        [HttpGet("release/{releaseId}/data-guidance")]
        public async Task<ActionResult<DataGuidanceViewModel>> Get(Guid releaseId)
        {
            return await _dataGuidanceService.Get(releaseId)
                .HandleFailuresOrOk();
        }

        [HttpPatch("release/{releaseId}/data-guidance")]
        public async Task<ActionResult<DataGuidanceViewModel>> Update(Guid releaseId,
            DataGuidanceUpdateViewModel request)
        {
            return await _dataGuidanceService.Update(releaseId, request)
                .HandleFailuresOrOk();
        }
    }
}
using System;
using System.Threading.Tasks;
using GovUk.Education.ExploreEducationStatistics.Admin.Services.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Admin.ViewModels;
using GovUk.Education.ExploreEducationStatistics.Common.Extensions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace GovUk.Education.ExploreEducationStatistics.Admin.Controllers.Api
{
    // TODO remove this
    [AllowAnonymous]
    [ApiController]
    public class MetaGuidanceController : ControllerBase
    {
        private readonly IMetaGuidanceService _metaGuidanceService;

        public MetaGuidanceController(IMetaGuidanceService metaGuidanceService)
        {
            _metaGuidanceService = metaGuidanceService;
        }

        [HttpGet("meta-guidance/{releaseId}")]
        public async Task<ActionResult<MetaGuidanceViewModel>> Get(Guid releaseId)
        {
            return await _metaGuidanceService.Get(releaseId)
                .HandleFailuresOrOk();
        }

        [HttpPut("meta-guidance/{releaseId}")]
        public async Task<ActionResult<MetaGuidanceViewModel>> Update(Guid releaseId, MetaGuidanceUpdateViewModel request)
        {
            return await _metaGuidanceService.Update(releaseId, request)
                .HandleFailuresOrOk();
        }
    }
}
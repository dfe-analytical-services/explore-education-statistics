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
    public class MetaGuidanceController : ControllerBase
    {
        private readonly IMetaGuidanceService _metaGuidanceService;

        public MetaGuidanceController(IMetaGuidanceService metaGuidanceService)
        {
            _metaGuidanceService = metaGuidanceService;
        }

        [HttpGet("release/{releaseId}/meta-guidance")]
        public async Task<ActionResult<MetaGuidanceViewModel>> Get(Guid releaseId)
        {
            return await _metaGuidanceService.Get(releaseId)
                .HandleFailuresOrOk();
        }

        [HttpPut("release/{releaseId}/meta-guidance")]
        public async Task<ActionResult<MetaGuidanceViewModel>> UpdateRelease(Guid releaseId,
            MetaGuidanceUpdateReleaseViewModel request)
        {
            return await _metaGuidanceService.UpdateRelease(releaseId, request)
                .HandleFailuresOrOk();
        }

        [HttpPut("release/{releaseId}/meta-guidance/subject/{subjectId}")]
        public async Task<ActionResult<MetaGuidanceViewModel>> UpdateSubject(Guid releaseId,
            Guid subjectId,
            MetaGuidanceUpdateSubjectViewModel request)
        {
            return await _metaGuidanceService.UpdateSubject(releaseId, subjectId, request)
                .HandleFailuresOrOk();
        }
    }
}
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using GovUk.Education.ExploreEducationStatistics.Admin.Services.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Admin.ViewModels;
using GovUk.Education.ExploreEducationStatistics.Common.Extensions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace GovUk.Education.ExploreEducationStatistics.Admin.Controllers.Api
{
    [Authorize]
    [ApiController]
    [Route("api")]
    public class LegacyReleaseController : ControllerBase
    {
        private readonly ILegacyReleaseService _legacyReleaseService;

        public LegacyReleaseController(ILegacyReleaseService legacyReleaseService)
        {
            _legacyReleaseService = legacyReleaseService;
        }

        [HttpGet("publications/{publicationId:guid}/legacy-releases")]
        public async Task<ActionResult<List<LegacyReleaseViewModel>>> ListLegacyReleases(Guid publicationId)
        {
            return await _legacyReleaseService
                .ListLegacyReleases(publicationId)
                .HandleFailuresOrOk();
        }

        [HttpGet("legacy-releases/{id:guid}")]
        public async Task<ActionResult<LegacyReleaseViewModel>> GetLegacyRelease(Guid id)
        {
            return await _legacyReleaseService
                .GetLegacyRelease(id)
                .HandleFailuresOrOk();
        }

        [HttpPost("legacy-releases")]
        public async Task<ActionResult<LegacyReleaseViewModel>> CreateLegacyRelease(
            LegacyReleaseCreateViewModel legacyRelease)
        {
            return await _legacyReleaseService
                .CreateLegacyRelease(legacyRelease)
                .HandleFailuresOrOk();
        }

        [HttpPut("legacy-releases/{id:guid}")]
        public async Task<ActionResult<LegacyReleaseViewModel>> UpdateLegacyRelease(
            Guid id,
            LegacyReleaseUpdateViewModel legacyRelease)
        {
            return await _legacyReleaseService
                .UpdateLegacyRelease(id, legacyRelease)
                .HandleFailuresOrOk();
        }

        [HttpDelete("legacy-releases/{id:guid}")]
        public async Task<ActionResult> DeleteLegacyRelease(Guid id)
        {
            return await _legacyReleaseService
                .DeleteLegacyRelease(id)
                .HandleFailuresOrNoContent();
        }
    }
}

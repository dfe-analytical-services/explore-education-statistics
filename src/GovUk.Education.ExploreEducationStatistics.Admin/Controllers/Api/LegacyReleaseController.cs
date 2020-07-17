using System;
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
    public class LegacyReleaseController : ControllerBase
    {
        private readonly ILegacyReleaseService _legacyReleaseService;

        public LegacyReleaseController(ILegacyReleaseService legacyReleaseService)
        {
            _legacyReleaseService = legacyReleaseService;
        }
        
        [HttpGet("api/legacy-releases/{id}")]
        public async Task<ActionResult<LegacyReleaseViewModel>> GetLegacyRelease(Guid id)
        {
            return await _legacyReleaseService
                .GetLegacyRelease(id)
                .HandleFailuresOrOk();
        }

        [HttpPost("api/legacy-releases")]
        public async Task<ActionResult<LegacyReleaseViewModel>> CreateLegacyRelease(
            CreateLegacyReleaseViewModel legacyRelease)
        {
            return await _legacyReleaseService
                .CreateLegacyRelease(legacyRelease)
                .HandleFailuresOrOk();
        }

        [HttpPut("api/legacy-releases/{id}")]
        public async Task<ActionResult<LegacyReleaseViewModel>> UpdateLegacyRelease(
            Guid id,
            UpdateLegacyReleaseViewModel legacyRelease)
        {
            return await _legacyReleaseService
                .UpdateLegacyRelease(id, legacyRelease)
                .HandleFailuresOrOk();
        }

        [HttpDelete("api/legacy-releases/{id}")]
        public async Task<ActionResult> DeleteLegacyRelease(Guid id)
        {
            return await _legacyReleaseService
                .DeleteLegacyRelease(id)
                .HandleFailuresOrNoContent();
        }
    }
}
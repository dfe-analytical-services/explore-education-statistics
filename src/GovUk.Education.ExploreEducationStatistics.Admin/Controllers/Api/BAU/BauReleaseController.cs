using System;
using System.Threading.Tasks;
using GovUk.Education.ExploreEducationStatistics.Admin.Services.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Common.Extensions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace GovUk.Education.ExploreEducationStatistics.Admin.Controllers.Api.BAU
{
    [Route("api")]
    [ApiController]
    [Authorize]
    public class BauReleaseController : ControllerBase
    {
        private readonly IReleaseService _releaseService;

        public BauReleaseController(IReleaseService releaseService)
        {
            _releaseService = releaseService;
        }

        [HttpPut("bau/release/{releaseId}/publish/content")]
        public async Task<ActionResult<bool>> PublishContentAsync(Guid releaseId)
        {
            return await _releaseService
                .PublishContentAsync(releaseId)
                .HandleFailuresOrOk();
        }
    }
}
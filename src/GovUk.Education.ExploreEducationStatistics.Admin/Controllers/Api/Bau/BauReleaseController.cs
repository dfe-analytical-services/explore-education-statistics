using System;
using System.Threading.Tasks;
using GovUk.Education.ExploreEducationStatistics.Admin.Services.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Common.Extensions;
using GovUk.Education.ExploreEducationStatistics.Common.Model;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace GovUk.Education.ExploreEducationStatistics.Admin.Controllers.Api.Bau
{
    [Route("api")]
    [ApiController]
    [Authorize]
    public class BauReleaseController : ControllerBase
    {
        private readonly IPublishingService _publishingService;

        public BauReleaseController(IPublishingService publishingService)
        {
            _publishingService = publishingService;
        }

        /// <summary>
        /// Retry a combination of the Content and Publishing stages of the publishing workflow.
        /// </summary>
        /// <param name="releaseId"></param>
        /// <returns></returns>
        [HttpPut("bau/release/{releaseId}/publish/content")]
        public async Task<ActionResult<Unit>> RetryReleasePublishing(Guid releaseId)
        {
            return await _publishingService
                .RetryReleasePublishing(releaseId)
                .HandleFailuresOrOk();
        }
    }
}
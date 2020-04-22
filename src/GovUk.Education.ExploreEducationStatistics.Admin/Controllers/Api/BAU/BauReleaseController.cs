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

        /**
         * Publish a Release immediately.
         * Runs all of the stages of the publishing workflow except that they are combined to act immediately.
         * Intended to be used with a Release that's already approved but not necessarily published.
         * Since the Data task deletes all existing Release statistics data before copying there will be downtime if this Release is already published.
         * If the Release is already published consider creating a new version instead.
         * Alternatively, republish only the content.
         * Publishing will fail at the validation stage if this Release is already in the process of being published.
         * A future schedule for publishing the Release that's not yet started will be cancelled.
         */
        [HttpPut("bau/release/{releaseId}/publish")]
        public async Task<ActionResult<bool>> PublishReleaseAsync(Guid releaseId)
        {
            return await _releaseService
                .PublishReleaseAsync(releaseId)
                .HandleFailuresOrOk();
        }

        /**
         * Regenerate and publish the content for a Release immediately.
         * Note that this is a combination of the Content and Publishing stages of the publishing workflow.
         * It doesn't include the Files or Data stages so is intended to be used with a Release already published.
         */
        [HttpPut("bau/release/{releaseId}/publish/content")]
        public async Task<ActionResult<bool>> PublishReleaseContentAsync(Guid releaseId)
        {
            return await _releaseService
                .PublishReleaseContentAsync(releaseId)
                .HandleFailuresOrOk();
        }
    }
}
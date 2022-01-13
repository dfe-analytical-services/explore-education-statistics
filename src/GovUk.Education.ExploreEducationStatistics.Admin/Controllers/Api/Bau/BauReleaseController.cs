using System;
using System.Threading.Tasks;
using GovUk.Education.ExploreEducationStatistics.Admin.Services.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Common.Extensions;
using GovUk.Education.ExploreEducationStatistics.Common.Model;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using static GovUk.Education.ExploreEducationStatistics.Publisher.Model.RetryStage;

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
        public async Task<ActionResult<Unit>> RetryContentAndPublishing(Guid releaseId)
        {
            return await _publishingService
                .RetryReleaseStage(releaseId, ContentAndPublishing)
                .HandleFailuresOrOk();
        }

        /// <summary>
        /// Retry the Data stage of the publishing workflow.
        /// </summary>
        /// <remarks>
        /// The outcome of this will depend on whether the latest attempt was Immediate,
        /// in which case a successful retry will trigger the remainder of the publishing workflow which is Content and Publishing.
        /// </remarks>
        /// <param name="releaseId"></param>
        /// <returns></returns>
        [HttpPut("bau/release/{releaseId}/publish/data")]
        public async Task<ActionResult<Unit>> RetryStatisticsData(Guid releaseId)
        {
            return await _publishingService
                .RetryReleaseStage(releaseId, StatisticsData)
                .HandleFailuresOrOk();
        }
    }
}
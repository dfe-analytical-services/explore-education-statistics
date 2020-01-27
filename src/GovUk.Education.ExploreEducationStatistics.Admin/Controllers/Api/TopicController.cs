using System;
using System.Threading.Tasks;
using GovUk.Education.ExploreEducationStatistics.Admin.Controllers.Utils;
using GovUk.Education.ExploreEducationStatistics.Admin.Models.Api;
using GovUk.Education.ExploreEducationStatistics.Admin.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace GovUk.Education.ExploreEducationStatistics.Admin.Controllers.Api
{
    /**
     * This controller is currently used by the test scripts for the purposes of creating data
     */
    [Route("api")]
    [ApiController]
    [Authorize(Roles = "BAU User")]
    public class TopicController : ControllerBase
    {
        private readonly ITopicService _topicService;

        public TopicController(ITopicService topicService)
        {
            _topicService = topicService;
        }
        
        // POST api/theme/{themeId}/topics
        [HttpPost("theme/{themeId}/topics")]
        public async Task<ActionResult<TopicViewModel>> CreateTopicAsync(
            CreateTopicRequest topic, Guid themeId)
        {
            return await _topicService
                .CreateTopicAsync(themeId, topic)
                .HandleFailuresOr(Ok);
        }
    }
}
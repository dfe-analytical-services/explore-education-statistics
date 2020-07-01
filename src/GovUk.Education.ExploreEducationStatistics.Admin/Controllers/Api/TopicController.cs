using System;
using System.Threading.Tasks;
using GovUk.Education.ExploreEducationStatistics.Admin.Models.Api;
using GovUk.Education.ExploreEducationStatistics.Admin.Services.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Common.Extensions;
using Microsoft.AspNetCore.Mvc;

namespace GovUk.Education.ExploreEducationStatistics.Admin.Controllers.Api
{
    /**
     * This controller is currently used by the test scripts for the purposes of creating data
     */
    [Route("api")]
    [ApiController]
    public class TopicController : ControllerBase
    {
        private readonly ITopicService _topicService;

        public TopicController(ITopicService topicService)
        {
            _topicService = topicService;
        }
        
        // POST api/theme/{themeId}/topics
        [HttpPost("theme/{themeId}/topics")]
        public async Task<ActionResult<TopicViewModel>> CreateTopic(
            CreateTopicRequest topic, Guid themeId)
        {
            return await _topicService
                .CreateTopic(themeId, topic)
                .HandleFailuresOrOk();
        }
    }
}
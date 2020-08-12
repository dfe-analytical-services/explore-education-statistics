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
    public class TopicController : ControllerBase
    {
        private readonly ITopicService _topicService;

        public TopicController(ITopicService topicService)
        {
            _topicService = topicService;
        }
        
        [HttpPost("theme/{themeId}/topics")]
        public async Task<ActionResult<TopicViewModel>> CreateTopic(
            CreateTopicRequest topic, Guid themeId)
        {
            return await _topicService
                .CreateTopic(themeId, topic)
                .HandleFailuresOrOk();
        }

        [HttpGet("topics/{topicId}")]
        public async Task<ActionResult<TopicViewModel>> GetTopic(Guid topicId)
        {
            return await _topicService
                .GetTopic(topicId)
                .HandleFailuresOrOk();
        }
    }
}
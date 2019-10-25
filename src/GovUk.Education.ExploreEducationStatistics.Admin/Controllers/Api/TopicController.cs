using System;
using System.Threading.Tasks;
using GovUk.Education.ExploreEducationStatistics.Admin.Services.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Content.Model;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace GovUk.Education.ExploreEducationStatistics.Admin.Controllers.Api
{
    [Route("api")]
    [ApiController]
    [Authorize]
    public class TopicController : ControllerBase
    {
        private ITopicService _topicService;
        
        public TopicController(ITopicService topicService) {}
        
        
        [HttpGet("topic/{topicId}/")]
        public async Task<ActionResult<Topic>> GetDataBlocksAsync(Guid topicId)
        {
            var topic = await _topicService.GetTopicAsync(topicId);

            if (topic == null)
            {
                return NotFound();
            }

            return Ok(topic);
        }
    }
}
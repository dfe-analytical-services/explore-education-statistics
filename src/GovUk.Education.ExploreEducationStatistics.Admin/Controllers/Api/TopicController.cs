using System;
using System.Threading.Tasks;
using GovUk.Education.ExploreEducationStatistics.Admin.Models.Api;
using GovUk.Education.ExploreEducationStatistics.Admin.Services.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Admin.Validators;
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
        private readonly ITopicService _topicService;

        public TopicController(ITopicService topicService)
        {
            _topicService = topicService;
        }
        
        
        [HttpGet("topic/{topicId}/")]
        public async Task<ActionResult<Topic>> GetTopicAsync(Guid topicId)
        {
            var topic = await _topicService.GetTopicAsync(topicId);

            if (topic == null)
            {
                return NotFound();
            }

            return Ok(topic);
        }

        // POST api/theme/{themeId}/topics
        [HttpPost("theme/{themeId}/topics")]
        public async Task<ActionResult<TopicViewModel>> CreateTopicRequest(
            CreateTopicViewModel topic, Guid themeId)
        {
           var result = await _topicService.CreateTopicRequest(themeId, topic);
           if (result.IsLeft)
           {
               ValidationUtils.AddErrors(ModelState, result.Left);
               return ValidationProblem(new ValidationProblemDetails(ModelState));
           }

           return result.Right;
        }
    }
}
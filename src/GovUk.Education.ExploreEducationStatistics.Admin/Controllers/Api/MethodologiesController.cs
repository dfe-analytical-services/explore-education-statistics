using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using GovUk.Education.ExploreEducationStatistics.Admin.Models.Api;
using GovUk.Education.ExploreEducationStatistics.Admin.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TopicId = System.Guid;

namespace GovUk.Education.ExploreEducationStatistics.Admin.Controllers.Api
{
    [ApiController]
    [Authorize]
    public class MethodologyController : ControllerBase
    {
        private readonly IMethodologyService _methodologyService;

        public MethodologyController(IMethodologyService methodologyService)
        {
            _methodologyService = methodologyService;
        }

        // GET api/topic/{topicId}/methodologies
        [HttpGet("/topic/{topicId}/methodologies")]
        [AllowAnonymous] // TODO revisit when authentication and authorisation is in place
        public ActionResult<List<MethodologyViewModel>> GetTopicMethodologies([Required]TopicId topicId)
        {
            return _methodologyService.GetTopicMethodologies(topicId);
        }
        
        [HttpGet("/methodologies")]
        [AllowAnonymous] // TODO revisit when authentication and authorisation is in place
        public ActionResult<List<MethodologyViewModel>> GetMethodologies()
        {
            return _methodologyService.List();
        }
        
    }
}
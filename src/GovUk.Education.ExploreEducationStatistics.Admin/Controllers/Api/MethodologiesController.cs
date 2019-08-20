using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Threading.Tasks;
using GovUk.Education.ExploreEducationStatistics.Admin.Models.Api;
using GovUk.Education.ExploreEducationStatistics.Admin.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TopicId = System.Guid;

namespace GovUk.Education.ExploreEducationStatistics.Admin.Controllers.Api
{
    [Authorize]
    [ApiController]
    public class MethodologiesController : ControllerBase
    {
        private readonly IMethodologyService _methodologyService;

        public MethodologiesController(IMethodologyService methodologyService)
        {
            _methodologyService = methodologyService;
        }

        // GET api/topic/{topicId}/methodologies
        [HttpGet("api/topic/{topicId}/methodologies")]
        public async Task<ActionResult<List<MethodologyViewModel>>> GetTopicMethodologiesAsync([Required]TopicId topicId)
        {
            return await _methodologyService.GetTopicMethodologiesAsync(topicId);
        }
        
        [HttpGet("api/methodologies")]
        public async Task<ActionResult<List<MethodologyViewModel>>> GetMethodologiesAsync()
        {
            return await _methodologyService.ListAsync();
        }
        
    }
}
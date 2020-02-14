using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Threading.Tasks;
using GovUk.Education.ExploreEducationStatistics.Admin.Controllers.Utils;
using GovUk.Education.ExploreEducationStatistics.Admin.Models.Api;
using GovUk.Education.ExploreEducationStatistics.Admin.Services.Interfaces.Methodologies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace GovUk.Education.ExploreEducationStatistics.Admin.Controllers.Api.Methodologies
{
    [Authorize]
    [ApiController]
    public class MethodologyController : ControllerBase
    {
        private readonly IMethodologyService _methodologyService;

        public MethodologyController(IMethodologyService methodologyService)
        {
            _methodologyService = methodologyService;
        }

        [Produces("application/json")]
        [HttpGet("api/topic/{topicId}/methodologies")]
        public async Task<ActionResult<List<MethodologyViewModel>>> GetTopicMethodologiesAsync([Required]Guid topicId)
        {
            return await _methodologyService
                .GetTopicMethodologiesAsync(topicId)
                .HandleFailuresOr(Ok);
        }
        
        [Produces("application/json")]
        [HttpGet("api/methodologies")]
        public async Task<ActionResult<List<MethodologyViewModel>>> GetMethodologiesAsync()
        {
            return await _methodologyService
                .ListAsync()
                .HandleFailuresOr(Ok);
        }
        
        [Produces("application/json")]
        [ProducesResponseType(typeof(MethodologyViewModel), 200)]
        [ProducesResponseType(typeof(ValidationProblemDetails), 400)]
        [HttpPost("api/methodologies")]
        public async Task<ActionResult<MethodologyViewModel>> CreateMethodologyAsync(
            CreateMethodologyViewModel methodology)
        {
            return await _methodologyService
                .CreateMethodologyAsync(methodology)
                .HandleFailuresOr(Ok);
        }
        
        [Produces("application/json")]
        [HttpGet("api/methodologies/summary")]
        public async Task<ActionResult<MethodologyViewModel>> GetMethodologySummaryAsync(Guid methodologyId)
        {
            return await _methodologyService
                .GetAsync(methodologyId)
                .HandleFailuresOr(Ok);
        }
        
    }
}
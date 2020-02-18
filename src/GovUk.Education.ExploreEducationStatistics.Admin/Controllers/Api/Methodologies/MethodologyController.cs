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
        public Task<ActionResult<List<MethodologyViewModel>>> GetTopicMethodologiesAsync([Required]Guid topicId)
        {
            return _methodologyService
                .GetTopicMethodologiesAsync(topicId)
                .HandleFailuresOrOk();
        }
        
        [Produces("application/json")]
        [HttpGet("api/methodologies")]
        public Task<ActionResult<List<MethodologyViewModel>>> GetMethodologiesAsync()
        {
            return _methodologyService
                .ListAsync()
                .HandleFailuresOrOk();
        }
        
        [Produces("application/json")]
        [ProducesResponseType(typeof(MethodologyViewModel), 200)]
        [ProducesResponseType(typeof(ValidationProblemDetails), 400)]
        [HttpPost("api/methodologies")]
        public Task<ActionResult<MethodologyViewModel>> CreateMethodologyAsync(
            CreateMethodologyViewModel methodology)
        {
            return _methodologyService
                .CreateMethodologyAsync(methodology)
                .HandleFailuresOrOk();
        }
        
        [Produces("application/json")]
        [HttpGet("api/methodologies/summary")]
        public async Task<ActionResult<MethodologyViewModel>> GetMethodologySummaryAsync(Guid methodologyId)
        {
            return await _methodologyService
                .GetAsync(methodologyId)
                .HandleFailuresOr(Ok);
        }
        
        [Produces("application/json")]
        [HttpGet("api/methodology/{methodologyId}")]
        public async Task<ActionResult<MethodologyViewModel>> GetMethodologySummaryAsync(Guid methodologyId, 
            UpdateMethodologyStatusRequest request)
        {
            return await _methodologyService
                .UpdateMethodologyStatusAsync(methodologyId, request)
                .HandleFailuresOr(Ok);
        }
    }
}
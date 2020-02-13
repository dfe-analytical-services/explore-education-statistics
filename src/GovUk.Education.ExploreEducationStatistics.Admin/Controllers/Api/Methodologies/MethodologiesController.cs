using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Threading.Tasks;
using GovUk.Education.ExploreEducationStatistics.Admin.Models.Api;
using GovUk.Education.ExploreEducationStatistics.Admin.Services.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Admin.Validators;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace GovUk.Education.ExploreEducationStatistics.Admin.Controllers.Api.Methodologies
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

        [Produces("application/json")]
        [HttpGet("api/topic/{topicId}/methodologies")]
        public async Task<ActionResult<List<MethodologyViewModel>>> GetTopicMethodologiesAsync([Required]Guid topicId)
        {
            return await _methodologyService.GetTopicMethodologiesAsync(topicId);
        }
        
        [Produces("application/json")]
        [HttpGet("api/methodologies")]
        public async Task<ActionResult<List<MethodologyViewModel>>> GetMethodologiesAsync()
        {
            return await _methodologyService.ListAsync();
        }
        
        [Produces("application/json")]
        [ProducesResponseType(typeof(MethodologyViewModel), 200)]
        [ProducesResponseType(typeof(ValidationProblemDetails), 400)]
        [HttpPost("api/methodologies")]
        public async Task<ActionResult<MethodologyViewModel>> CreateMethodologyAsync(
            CreateMethodologyViewModel methodology)
        {
            var result = await _methodologyService.CreateMethodologyAsync(methodology);
            
            if (result.IsLeft)
            {
                ValidationUtils.AddErrors(ModelState, result.Left);
                return ValidationProblem(new ValidationProblemDetails(ModelState));
            }

            return result.Right;
        }
        
        [Produces("application/json")]
        [HttpGet("api/methodologies/summary")]
        public async Task<ActionResult<MethodologyViewModel>> GetMethodologySummaryAsync(Guid methodologyId)
        {
            return await _methodologyService.GetAsync(methodologyId);
        }
        
    }
}
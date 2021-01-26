using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using GovUk.Education.ExploreEducationStatistics.Admin.Services.Interfaces.Methodologies;
using GovUk.Education.ExploreEducationStatistics.Admin.ViewModels;
using GovUk.Education.ExploreEducationStatistics.Admin.ViewModels.Methodology;
using GovUk.Education.ExploreEducationStatistics.Common.Extensions;
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
        [HttpGet("api/methodologies")]
        public Task<ActionResult<List<MethodologySummaryViewModel>>> GetMethodologiesAsync()
        {
            return _methodologyService
                .ListAsync()
                .HandleFailuresOrOk();
        }

        [Produces("application/json")]
        [ProducesResponseType(typeof(MethodologySummaryViewModel), 200)]
        [ProducesResponseType(typeof(ValidationProblemDetails), 400)]
        [HttpPost("api/methodologies")]
        public Task<ActionResult<MethodologySummaryViewModel>> CreateMethodologyAsync(
            MethodologyCreateRequest methodology)
        {
            return _methodologyService
                .CreateMethodologyAsync(methodology)
                .HandleFailuresOrOk();
        }

        [Produces("application/json")]
        [HttpGet("api/methodology/{methodologyId}/summary")]
        public async Task<ActionResult<MethodologySummaryViewModel>> GetMethodologySummaryAsync(Guid methodologyId)
        {
            return await _methodologyService
                .GetSummaryAsync(methodologyId)
                .HandleFailuresOrOk();
        }

        [Produces("application/json")]
        [HttpPut("api/methodology/{methodologyId}")]
        public async Task<ActionResult<MethodologySummaryViewModel>> UpdateMethodologyAsync(Guid methodologyId,
            MethodologyUpdateRequest request)
        {
            return await _methodologyService
                .UpdateMethodologyAsync(methodologyId, request)
                .HandleFailuresOrOk();
        }
        
        [HttpGet("api/me/methodologies")]
        public async Task<ActionResult<List<MethodologyPublicationsViewModel>>> GetMyMethodologyList()
        {
            return await _methodologyService
                .ListWithPublicationsAsync()
                .HandleFailuresOr(Ok);
        }
    }
}

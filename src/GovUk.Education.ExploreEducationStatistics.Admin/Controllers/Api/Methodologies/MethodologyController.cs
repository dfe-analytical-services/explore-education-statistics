using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using GovUk.Education.ExploreEducationStatistics.Admin.Services.Interfaces.Methodologies;
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
        [ProducesResponseType(typeof(MethodologySummaryViewModel), 200)]
        [ProducesResponseType(typeof(ValidationProblemDetails), 400)]
        [HttpPost("api/publication/{publicationId}/methodology")]
        public Task<ActionResult<MethodologySummaryViewModel>> CreateMethodology(Guid publicationId)
        {
            return _methodologyService
                .CreateMethodology(publicationId)
                .HandleFailuresOrOk();
        }
        
        [Produces("application/json")]
        [HttpGet("api/methodology/{methodologyId}/summary")]
        public async Task<ActionResult<MethodologySummaryViewModel>> GetMethodologySummary(Guid methodologyId)
        {
            return await _methodologyService
                .GetSummary(methodologyId)
                .HandleFailuresOrOk();
        }

        [Produces("application/json")]
        [HttpPut("api/methodology/{methodologyId}")]
        public async Task<ActionResult<MethodologySummaryViewModel>> UpdateMethodology(Guid methodologyId,
            MethodologyUpdateRequest request)
        {
            return await _methodologyService
                .UpdateMethodology(methodologyId, request)
                .HandleFailuresOrOk();
        }

        [HttpGet("api/me/methodologies")]
        public async Task<ActionResult<List<MethodologyPublicationsViewModel>>> GetMyMethodologyList()
        {
            return await _methodologyService
                .ListWithPublications()
                .HandleFailuresOrOk();
        }
    }
}

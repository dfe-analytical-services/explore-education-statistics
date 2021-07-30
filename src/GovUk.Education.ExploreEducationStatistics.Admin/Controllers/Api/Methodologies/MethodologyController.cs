using System;
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
        private readonly IMethodologyAmendmentService _methodologyAmendmentService;

        public MethodologyController(
            IMethodologyService methodologyService, 
            IMethodologyAmendmentService methodologyAmendmentService)
        {
            _methodologyService = methodologyService;
            _methodologyAmendmentService = methodologyAmendmentService;
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

        [Produces("application/json")]
        [ProducesResponseType(typeof(MethodologySummaryViewModel), 200)]
        [ProducesResponseType(typeof(ValidationProblemDetails), 400)]
        [HttpPost("api/methodology/{methodologyId}/amendment")]
        public Task<ActionResult<MethodologySummaryViewModel>> CreateMethodologyAmendment(Guid methodologyId)
        {
            return _methodologyAmendmentService
                .CreateMethodologyAmendment(methodologyId)
                .HandleFailuresOrOk();
        }

        [HttpDelete("api/methodology/{methodologyId}")]
        public Task<ActionResult> DeleteMethodology(Guid methodologyId)
        {
            return _methodologyService
                .DeleteMethodology(methodologyId)
                .HandleFailuresOrNoContent();
        }
    }
}

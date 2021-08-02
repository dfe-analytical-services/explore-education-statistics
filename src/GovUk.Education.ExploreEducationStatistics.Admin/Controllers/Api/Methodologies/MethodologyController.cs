#nullable enable
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
    [Route("api")]
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

        [HttpPost("publication/{publicationId}/methodology")]
        public Task<ActionResult<MethodologySummaryViewModel>> CreateMethodology(Guid publicationId)
        {
            return _methodologyService
                .CreateMethodology(publicationId)
                .HandleFailuresOrOk();
        }

        [HttpGet("methodology/{methodologyId}/summary")]
        public async Task<ActionResult<MethodologySummaryViewModel>> GetMethodologySummary(Guid methodologyId)
        {
            return await _methodologyService
                .GetSummary(methodologyId)
                .HandleFailuresOrOk();
        }

        [HttpGet("methodology/{methodologyId}/unpublished-releases")]
        public async Task<ActionResult<List<TitleAndIdViewModel>>> GetUnpublishedReleasesUsingMethodology(
            Guid methodologyId)
        {
            return await _methodologyService
                .GetUnpublishedReleasesUsingMethodology(methodologyId)
                .HandleFailuresOrOk();
        }

        [HttpPut("methodology/{methodologyId}")]
        public async Task<ActionResult<MethodologySummaryViewModel>> UpdateMethodology(Guid methodologyId,
            MethodologyUpdateRequest request)
        {
            return await _methodologyService
                .UpdateMethodology(methodologyId, request)
                .HandleFailuresOrOk();
        }

        [HttpPost("methodology/{methodologyId}/amendment")]
        public Task<ActionResult<MethodologySummaryViewModel>> CreateMethodologyAmendment(Guid methodologyId)
        {
            return _methodologyAmendmentService
                .CreateMethodologyAmendment(methodologyId)
                .HandleFailuresOrOk();
        }

        [HttpDelete("methodology/{methodologyId}")]
        public Task<ActionResult> DeleteMethodology(Guid methodologyId)
        {
            return _methodologyService
                .DeleteMethodology(methodologyId)
                .HandleFailuresOrNoContent();
        }
    }
}

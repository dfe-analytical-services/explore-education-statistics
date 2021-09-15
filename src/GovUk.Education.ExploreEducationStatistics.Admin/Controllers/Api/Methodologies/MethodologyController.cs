#nullable enable
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using GovUk.Education.ExploreEducationStatistics.Admin.Services.Interfaces.Methodologies;
using GovUk.Education.ExploreEducationStatistics.Admin.ViewModels;
using GovUk.Education.ExploreEducationStatistics.Admin.ViewModels.Methodology;
using GovUk.Education.ExploreEducationStatistics.Common.Extensions;
using GovUk.Education.ExploreEducationStatistics.Common.Model;
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

        [HttpPut("publication/{publicationId}/methodology/{methodologyId}")]
        public async Task<ActionResult<Unit>> AdoptMethodology(Guid publicationId, Guid methodologyId)
        {
            return await _methodologyService
                .AdoptMethodology(publicationId, methodologyId)
                .HandleFailuresOrOk();
        }

        [HttpPost("publication/{publicationId}/methodology")]
        public Task<ActionResult<MethodologySummaryViewModel>> CreateMethodology(Guid publicationId)
        {
            return _methodologyService
                .CreateMethodology(publicationId)
                .HandleFailuresOrOk();
        }

        [HttpDelete("publication/{publicationId}/methodology/{methodologyId}")]
        public async Task<ActionResult> DropMethodology(Guid publicationId, Guid methodologyId)
        {
            return await _methodologyService
                .DropMethodology(publicationId, methodologyId)
                .HandleFailuresOrNoContent();
        }

        [HttpGet("publication/{publicationId}/adoptable-methodologies")]
        public async Task<ActionResult<List<MethodologySummaryViewModel>>> GetAdoptableMethodologies(Guid publicationId)
        {
            return await _methodologyService
                .GetAdoptableMethodologies(publicationId)
                .HandleFailuresOrOk();
        }

        [HttpGet("methodology/{methodologyVersionId}/summary")]
        public async Task<ActionResult<MethodologySummaryViewModel>> GetMethodologySummary(Guid methodologyVersionId)
        {
            return await _methodologyService
                .GetSummary(methodologyVersionId)
                .HandleFailuresOrOk();
        }

        [HttpGet("methodology/{methodologyVersionId}/unpublished-releases")]
        public async Task<ActionResult<List<TitleAndIdViewModel>>> GetUnpublishedReleasesUsingMethodology(
            Guid methodologyVersionId)
        {
            return await _methodologyService
                .GetUnpublishedReleasesUsingMethodology(methodologyVersionId)
                .HandleFailuresOrOk();
        }

        [HttpPut("methodology/{methodologyVersionId}")]
        public async Task<ActionResult<MethodologySummaryViewModel>> UpdateMethodology(
            Guid methodologyVersionId,
            MethodologyUpdateRequest request)
        {
            return await _methodologyService
                .UpdateMethodology(methodologyVersionId, request)
                .HandleFailuresOrOk();
        }

        [HttpPost("methodology/{originalMethodologyVersionId}/amendment")]
        public Task<ActionResult<MethodologySummaryViewModel>> CreateMethodologyAmendment(
            Guid originalMethodologyVersionId)
        {
            return _methodologyAmendmentService
                .CreateMethodologyAmendment(originalMethodologyVersionId)
                .HandleFailuresOrOk();
        }

        [HttpDelete("methodology/{methodologyVersionId}")]
        public Task<ActionResult> DeleteMethodologyVersion(Guid methodologyVersionId)
        {
            return _methodologyService
                .DeleteMethodologyVersion(methodologyVersionId)
                .HandleFailuresOrNoContent();
        }
    }
}

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

        [HttpPut("publication/{publicationId:guid}/methodology/{methodologyId}")]
        public async Task<ActionResult<Unit>> AdoptMethodology(Guid publicationId, Guid methodologyId)
        {
            return await _methodologyService
                .AdoptMethodology(publicationId, methodologyId)
                .HandleFailuresOrOk();
        }

        [HttpPost("publication/{publicationId:guid}/methodology")]
        public Task<ActionResult<MethodologyVersionViewModel>> CreateMethodology(Guid publicationId)
        {
            return _methodologyService
                .CreateMethodology(publicationId)
                .HandleFailuresOrOk();
        }

        [HttpDelete("publication/{publicationId:guid}/methodology/{methodologyId}")]
        public async Task<ActionResult> DropMethodology(Guid publicationId, Guid methodologyId)
        {
            return await _methodologyService
                .DropMethodology(publicationId, methodologyId)
                .HandleFailuresOrNoContent();
        }

        [HttpGet("publication/{publicationId:guid}/adoptable-methodologies")]
        public async Task<ActionResult<List<MethodologyVersionViewModel>>> GetAdoptableMethodologies(Guid publicationId)
        {
            return await _methodologyService
                .GetAdoptableMethodologies(publicationId)
                .HandleFailuresOrOk();
        }

        [HttpGet("methodology/{methodologyVersionId:guid}")]
        public async Task<ActionResult<MethodologyVersionViewModel>> GetMethodology(Guid methodologyVersionId)
        {
            return await _methodologyService
                .GetMethodology(methodologyVersionId)
                .HandleFailuresOrOk();
        }

        [HttpGet("methodology/{methodologyVersionId:guid}/unpublished-releases")]
        public async Task<ActionResult<List<IdTitleViewModel>>> GetUnpublishedReleasesUsingMethodology(
            Guid methodologyVersionId)
        {
            return await _methodologyService
                .GetUnpublishedReleasesUsingMethodology(methodologyVersionId)
                .HandleFailuresOrOk();
        }

        [HttpGet("publication/{publicationId:guid}/methodologies")]
        public async Task<ActionResult<List<MethodologyVersionSummaryViewModel>>> ListMethodologies(Guid publicationId)
        {
            return await _methodologyService
                .ListMethodologies(publicationId)
                .HandleFailuresOrOk();
        }

        [HttpPut("methodology/{methodologyVersionId:guid}")]
        public async Task<ActionResult<MethodologyVersionViewModel>> UpdateMethodology(
            Guid methodologyVersionId,
            MethodologyUpdateRequest request)
        {
            return await _methodologyService
                .UpdateMethodology(methodologyVersionId, request)
                .HandleFailuresOrOk();
        }

        [HttpPost("methodology/{originalMethodologyVersionId:guid}/amendment")]
        public Task<ActionResult<MethodologyVersionViewModel>> CreateMethodologyAmendment(
            Guid originalMethodologyVersionId)
        {
            return _methodologyAmendmentService
                .CreateMethodologyAmendment(originalMethodologyVersionId)
                .HandleFailuresOrOk();
        }

        [HttpDelete("methodology/{methodologyVersionId:guid}")]
        public Task<ActionResult> DeleteMethodologyVersion(Guid methodologyVersionId)
        {
            return _methodologyService
                .DeleteMethodologyVersion(methodologyVersionId)
                .HandleFailuresOrNoContent();
        }
    }
}

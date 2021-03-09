using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using GovUk.Education.ExploreEducationStatistics.Admin.Services.Interfaces.ManageContent;
using GovUk.Education.ExploreEducationStatistics.Admin.ViewModels;
using GovUk.Education.ExploreEducationStatistics.Common.Extensions;
using GovUk.Education.ExploreEducationStatistics.Content.Model;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace GovUk.Education.ExploreEducationStatistics.Admin.Controllers.Api.ManageContent
{
    [Route("api")]
    [ApiController]
    [Authorize]
    public class RelatedInformationController : ControllerBase
    {
        private readonly IRelatedInformationService _relatedInformationService;

        public RelatedInformationController(IRelatedInformationService relatedInformationService)
        {
            _relatedInformationService = relatedInformationService;
        }

        [HttpGet("release/{releaseId}/content/related-information")]
        public async Task<ActionResult<List<Link>>> GetRelatedInformation(Guid releaseId)
        {
            return await _relatedInformationService
                .GetRelatedInformationAsync(releaseId)
                .HandleFailuresOrOk();
        }

        [HttpPost("release/{releaseId}/content/related-information")]
        public async Task<ActionResult<List<Link>>> AddRelatedInformation(CreateUpdateLinkRequest request, Guid releaseId)
        {
            return await _relatedInformationService
                .AddRelatedInformationAsync(releaseId, request)
                .HandleFailuresOr(result => Created(HttpContext.Request.Path, result));
        }

        [HttpPut("release/{releaseId}/content/related-information/{relatedInformationId}")]
        public async Task<ActionResult<List<Link>>> UpdateRelatedInformation(
            CreateUpdateLinkRequest request, Guid releaseId, Guid relatedInformationId)
        {
            return await _relatedInformationService
                .UpdateRelatedInformationAsync(releaseId, relatedInformationId, request)
                .HandleFailuresOrOk();
        }

        [HttpDelete("release/{releaseId}/content/related-information/{relatedInformationId}")]
        public async Task<ActionResult<List<Link>>> DeleteRelatedInformation(
            Guid releaseId, Guid relatedInformationId)
        {
            return await _relatedInformationService
                .DeleteRelatedInformationAsync(releaseId, relatedInformationId)
                .HandleFailuresOrOk();
        }
    }
}
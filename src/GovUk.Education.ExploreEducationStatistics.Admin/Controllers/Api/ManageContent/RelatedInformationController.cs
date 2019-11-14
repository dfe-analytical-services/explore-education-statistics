using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using GovUk.Education.ExploreEducationStatistics.Admin.Controllers.ExtensionMethods;
using GovUk.Education.ExploreEducationStatistics.Admin.Services.Interfaces.ManageContent;
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
        public Task<ActionResult<List<BasicLink>>> GetRelatedInformation(Guid releaseId)
        {
            return this.HandlingValidationErrorsAsync(
                () => _relatedInformationService.GetRelatedInformationAsync(releaseId),
                Ok);
        }
        
        [HttpPost("release/{releaseId}/content/related-information")]
        public Task<ActionResult<List<BasicLink>>> AddRelatedInformation(CreateUpdateLinkRequest request, Guid releaseId)
        {
            return this.HandlingValidationErrorsAsync(
                () => _relatedInformationService.AddRelatedInformationAsync(releaseId, request),
                result => Created(HttpContext.Request.Path, result));
        }
        
        [HttpPut("release/{releaseId}/content/related-information/{relatedInformationId}")]
        public Task<ActionResult<List<BasicLink>>> UpdateRelatedInformation(
            CreateUpdateLinkRequest request, Guid releaseId, Guid relatedInformationId)
        {
            return this.HandlingValidationErrorsAsync(
                () => _relatedInformationService.UpdateRelatedInformationAsync(releaseId, relatedInformationId,
                    request),
                Ok);
        }
        
        [HttpDelete("release/{releaseId}/content/related-information/{relatedInformationId}")]
        public Task<ActionResult<List<BasicLink>>> DeleteRelatedInformation(
            Guid releaseId, Guid relatedInformationId)
        {
            return this.HandlingValidationErrorsAsync(
                () => _relatedInformationService.DeleteRelatedInformationAsync(releaseId, relatedInformationId),
                Ok);
        }
    }
}
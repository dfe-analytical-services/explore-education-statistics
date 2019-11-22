using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using GovUk.Education.ExploreEducationStatistics.Admin.Controllers.ExtensionMethods;
using GovUk.Education.ExploreEducationStatistics.Admin.Services.Interfaces.ManageContent;
using GovUk.Education.ExploreEducationStatistics.Admin.ViewModels.ManageContent;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace GovUk.Education.ExploreEducationStatistics.Admin.Controllers.Api.ManageContent
{
    [Route("api")]
    [ApiController]
    [Authorize]
    public class ContentController : ControllerBase
    {
        private readonly IContentService _contentService;

        public ContentController(IContentService contentService)
        {
            _contentService = contentService;
        }

        [HttpGet("release/{releaseId}/content/sections")]
        public Task<ActionResult<List<ContentSectionViewModel>>> GetContentSections(Guid releaseId)
        {
            return this.HandlingValidationErrorsAsync(
                () => _contentService.GetContentSectionsAsync(releaseId),
                Ok);
        }
        
        [HttpPut("release/{releaseId}/content/sections/order")]
        public Task<ActionResult<List<ContentSectionViewModel>>> ReorderSections(Guid releaseId, Dictionary<Guid, int> newSectionOrder)
        {
            return this.HandlingValidationErrorsAsync(
                () => _contentService.ReorderContentSectionsAsync(releaseId, newSectionOrder),
                Ok);
        }

        [HttpPost("release/{releaseId}/content/sections/add")]
        public Task<ActionResult<ContentSectionViewModel>> AddContentSection(Guid releaseId)
        {
            return this.HandlingValidationErrorsAsync(
                () => _contentService.AddContentSectionAsync(releaseId),
                Ok);
        }

        [HttpPut("release/{releaseId}/content/section/{contentSectionId}/heading")]
        public Task<ActionResult<ContentSectionViewModel>> UpdateContentSectionHeading(Guid releaseId, Guid contentSectionId, UpdateContentSectionHeadingRequest request)
        {
            return this.HandlingValidationErrorsAsync(
                () => _contentService.UpdateContentSectionHeadingAsync(releaseId, contentSectionId, request.Heading),
                Ok);
        }

        [HttpDelete("release/{releaseId}/content/section/{contentSectionId}")]
        public Task<ActionResult<List<ContentSectionViewModel>>> RemoveContentSection(Guid releaseId, Guid contentSectionId)
        {
            return this.HandlingValidationErrorsAsync(
                () => _contentService.RemoveContentSectionAsync(releaseId, contentSectionId),
                Ok);
        }
    }
}
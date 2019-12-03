using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using GovUk.Education.ExploreEducationStatistics.Admin.Controllers.ExtensionMethods;
using GovUk.Education.ExploreEducationStatistics.Admin.Services.Interfaces.ManageContent;
using GovUk.Education.ExploreEducationStatistics.Admin.ViewModels.ManageContent;
using GovUk.Education.ExploreEducationStatistics.Content.Model;
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
        public Task<ActionResult<List<ContentSectionViewModel>>> ReorderSections(
            Guid releaseId, Dictionary<Guid, int> newSectionOrder)
        {
            return this.HandlingValidationErrorsAsync(
                () => _contentService.ReorderContentSectionsAsync(releaseId, newSectionOrder),
                Ok);
        }

        [HttpPost("release/{releaseId}/content/sections/add")]
        public Task<ActionResult<ContentSectionViewModel>> AddContentSection(
            Guid releaseId, AddContentSectionRequest? request = null)
        {
            return this.HandlingValidationErrorsAsync(
                () => _contentService.AddContentSectionAsync(releaseId, request),
                Ok);
        }

        [HttpPut("release/{releaseId}/content/section/{contentSectionId}/heading")]
        public Task<ActionResult<ContentSectionViewModel>> UpdateContentSectionHeading(
            Guid releaseId, Guid contentSectionId, UpdateContentSectionHeadingRequest request)
        {
            return this.HandlingValidationErrorsAsync(
                () => _contentService.UpdateContentSectionHeadingAsync(releaseId, contentSectionId, request.Heading),
                Ok);
        }

        [HttpDelete("release/{releaseId}/content/section/{contentSectionId}")]
        public Task<ActionResult<List<ContentSectionViewModel>>> RemoveContentSection(
            Guid releaseId, Guid contentSectionId)
        {
            return this.HandlingValidationErrorsAsync(
                () => _contentService.RemoveContentSectionAsync(releaseId, contentSectionId),
                Ok);
        }

        [HttpGet("release/{releaseId}/content/section/{contentSectionId}")]
        public Task<ActionResult<ContentSectionViewModel>> GetContentSection(Guid releaseId, Guid contentSectionId)
        {
            return this.HandlingValidationErrorsAsync(
                () => _contentService.GetContentSectionAsync(releaseId, contentSectionId),
                Ok);
        }
        
        [HttpPut("release/{releaseId}/content/section/{contentSectionId}/blocks/order")]
        public Task<ActionResult<List<IContentBlock>>> ReorderContentBlocks(
            Guid releaseId, Guid contentSectionId, Dictionary<Guid, int> newBlocksOrder)
        {
            return this.HandlingValidationErrorsAsync(
                () => _contentService.ReorderContentBlocksAsync(releaseId, contentSectionId, newBlocksOrder),
                Ok);
        }

        [HttpPost("release/{releaseId}/content/section/{contentSectionId}/blocks/add")]
        public Task<ActionResult<IContentBlock>> AddContentBlock(
            Guid releaseId, Guid contentSectionId, AddContentBlockRequest request)
        {
            return this.HandlingValidationErrorsAsync(
                () => _contentService.AddContentBlockAsync(releaseId, contentSectionId, request),
                Ok);
        }

        [HttpDelete("release/{releaseId}/content/section/{contentSectionId}/block/{contentBlockId}")]
        public Task<ActionResult<List<IContentBlock>>> RemoveContentBlock(
            Guid releaseId, Guid contentSectionId, Guid contentBlockId)
        {
            return this.HandlingValidationErrorsAsync(
                () => _contentService.RemoveContentBlockAsync(releaseId, contentSectionId, contentBlockId),
                Ok);
        }

        [HttpPut("release/{releaseId}/content/section/{contentSectionId}/block/{contentBlockId}")]
        public Task<ActionResult<IContentBlock>> UpdateTextBasedContentBlock(
            Guid releaseId, Guid contentSectionId, Guid contentBlockId, UpdateTextBasedContentBlockRequest request)
        {
            return this.HandlingValidationErrorsAsync(
                () => _contentService.UpdateTextBasedContentBlockAsync(
                    releaseId, contentSectionId, contentBlockId, request),
                Ok);
        }

        [HttpGet("release/{releaseId}/content/available-datablocks")]
        public Task<ActionResult<List<DataBlock>>> GetAvailableDataBlocks(Guid releaseId)
        {
            return this.HandlingValidationErrorsAsync(
                () => _contentService.GetUnattachedContentBlocksAsync<DataBlock>(releaseId),
                Ok);
        }

        [HttpPost("release/{releaseId}/content/section/{contentSectionId}/blocks/attach")]
        public Task<ActionResult<IContentBlock>> AddContentBlock(
            Guid releaseId, Guid contentSectionId, AttachContentBlockRequest request)
        {
            return this.HandlingValidationErrorsAsync(
                () => _contentService.AttachContentBlockAsync(releaseId, contentSectionId, request),
                Ok);
        }
    }
}
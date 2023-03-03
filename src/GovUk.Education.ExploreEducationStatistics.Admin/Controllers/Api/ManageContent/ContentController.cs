using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using GovUk.Education.ExploreEducationStatistics.Admin.Services.Interfaces.ManageContent;
using GovUk.Education.ExploreEducationStatistics.Admin.ViewModels;
using GovUk.Education.ExploreEducationStatistics.Admin.ViewModels.ManageContent;
using GovUk.Education.ExploreEducationStatistics.Common.Extensions;
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

        [HttpPut("release/{releaseId:guid}/content/sections/order")]
        public async Task<ActionResult<List<ContentSectionViewModel>>> ReorderSections(
            Guid releaseId, Dictionary<Guid, int> newSectionOrder)
        {
            return await _contentService
                .ReorderContentSections(releaseId, newSectionOrder)
                .HandleFailuresOrOk();
        }

        [HttpPost("release/{releaseId:guid}/content/sections/add")]
        public async Task<ActionResult<ContentSectionViewModel>> AddContentSection(
            Guid releaseId, ContentSectionAddRequest request = null)
        {
            return await _contentService
                .AddContentSectionAsync(releaseId, request)
                .HandleFailuresOrOk();
        }

        [HttpPut("release/{releaseId:guid}/content/section/{contentSectionId:guid}/heading")]
        public async Task<ActionResult<ContentSectionViewModel>> UpdateContentSectionHeading(
            Guid releaseId, Guid contentSectionId, ContentSectionHeadingUpdateRequest request)
        {
            return await _contentService
                .UpdateContentSectionHeading(releaseId, contentSectionId, request.Heading)
                .HandleFailuresOrOk();
        }

        [HttpDelete("release/{releaseId:guid}/content/section/{contentSectionId:guid}")]
        public async Task<ActionResult<List<ContentSectionViewModel>>> RemoveContentSection(
            Guid releaseId, Guid contentSectionId)
        {
            return await _contentService
                .RemoveContentSection(releaseId, contentSectionId)
                .HandleFailuresOrOk();
        }

        [HttpPut("release/{releaseId:guid}/content/section/{contentSectionId:guid}/blocks/order")]
        public async Task<ActionResult<List<IContentBlockViewModel>>> ReorderContentBlocks(
            Guid releaseId, Guid contentSectionId, Dictionary<Guid, int> newBlocksOrder)
        {
            return await _contentService
                .ReorderContentBlocks(releaseId, contentSectionId, newBlocksOrder)
                .HandleFailuresOrOk();
        }

        [HttpPost("release/{releaseId:guid}/content/section/{contentSectionId:guid}/blocks/add")]
        public async Task<ActionResult<IContentBlockViewModel>> AddContentBlock(
            Guid releaseId, Guid contentSectionId, ContentBlockAddRequest request)
        {
            return await _contentService
                .AddContentBlock(releaseId, contentSectionId, request)
                .HandleFailuresOrOk();
        }

        [HttpDelete("release/{releaseId:guid}/content/section/{contentSectionId:guid}/block/{contentBlockId:guid}")]
        public async Task<ActionResult<List<IContentBlockViewModel>>> RemoveContentBlock(
            Guid releaseId, Guid contentSectionId, Guid contentBlockId)
        {
            return await _contentService
                .RemoveContentBlock(releaseId, contentSectionId, contentBlockId)
                .HandleFailuresOrOk();
        }

        [HttpPut("release/{releaseId:guid}/content/section/{contentSectionId:guid}/block/{contentBlockId:guid}")]
        public async Task<ActionResult<IContentBlockViewModel>> UpdateTextBasedContentBlock(
            Guid releaseId, Guid contentSectionId, Guid contentBlockId, ContentBlockUpdateRequest request)
        {
            return await _contentService
                .UpdateTextBasedContentBlock(releaseId, contentSectionId, contentBlockId, request)
                .HandleFailuresOrOk();
        }

        [HttpPost("release/{releaseId:guid}/content/section/{contentSectionId:guid}/blocks/attach")]
        public async Task<ActionResult<IContentBlockViewModel>> AttachDataBlock(
            Guid releaseId, Guid contentSectionId, ContentBlockAttachRequest request)
        {
            return await _contentService
                .AttachDataBlock(releaseId, contentSectionId, request)
                .HandleFailuresOrOk();
        }
    }
}

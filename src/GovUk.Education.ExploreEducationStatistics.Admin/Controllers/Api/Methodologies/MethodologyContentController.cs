using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using GovUk.Education.ExploreEducationStatistics.Admin.Services.Interfaces.Methodologies;
using GovUk.Education.ExploreEducationStatistics.Admin.Services.Methodologies;
using GovUk.Education.ExploreEducationStatistics.Admin.ViewModels;
using GovUk.Education.ExploreEducationStatistics.Admin.ViewModels.ManageContent;
using GovUk.Education.ExploreEducationStatistics.Common.Extensions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace GovUk.Education.ExploreEducationStatistics.Admin.Controllers.Api.Methodologies
{
    [Route("api")]
    [ApiController]
    [Authorize]
    public class MethodologyContentController : ControllerBase
    {
        private readonly IMethodologyContentService _contentService;

        public MethodologyContentController(IMethodologyContentService contentService)
        {
            _contentService = contentService;
        }

        [HttpGet("methodology/{methodologyId}/content")]
        public async Task<ActionResult<ManageMethodologyContentViewModel>> GetContent(Guid methodologyId)
        {
            return await _contentService
                .GetContent(methodologyId)
                .HandleFailuresOrOk();
        }

        [HttpPut("methodology/{methodologyId}/content/sections/order")]
        public async Task<ActionResult<List<ContentSectionViewModel>>> ReorderSections(
            Guid methodologyId, Dictionary<Guid, int> newSectionOrder)
        {
            return await _contentService
                .ReorderContentSections(methodologyId, newSectionOrder)
                .HandleFailuresOrOk();
        }

        [HttpPost("methodology/{methodologyId}/content/sections/add")]
        public async Task<ActionResult<ContentSectionViewModel>> AddContentSection(
            Guid methodologyId,
            [FromQuery] MethodologyContentService.ContentListType type,
            ContentSectionAddRequest request = null)
        {
            return await _contentService
                .AddContentSection(methodologyId, request, type)
                .HandleFailuresOrOk();
        }

        [HttpPut("methodology/{methodologyId}/content/section/{contentSectionId}/heading")]
        public async Task<ActionResult<ContentSectionViewModel>> UpdateContentSectionHeading(
            Guid methodologyId, Guid contentSectionId, ContentSectionHeadingUpdateRequest request)
        {
            return await _contentService
                .UpdateContentSectionHeading(methodologyId, contentSectionId, request.Heading)
                .HandleFailuresOrOk();
        }

        [HttpDelete("methodology/{methodologyId}/content/section/{contentSectionId}")]
        public async Task<ActionResult<List<ContentSectionViewModel>>> RemoveContentSection(
            Guid methodologyId, Guid contentSectionId)
        {
            return await _contentService
                .RemoveContentSection(methodologyId, contentSectionId)
                .HandleFailuresOrOk();
        }

        [HttpGet("methodology/{methodologyId}/content/section/{contentSectionId}")]
        public async Task<ActionResult<ContentSectionViewModel>> GetContentSection(Guid methodologyId,
            Guid contentSectionId)
        {
            return await _contentService
                .GetContentSection(methodologyId, contentSectionId)
                .HandleFailuresOrOk();
        }

        [HttpPut("methodology/{methodologyId}/content/section/{contentSectionId}/blocks/order")]
        public async Task<ActionResult<List<IContentBlockViewModel>>> ReorderContentBlocks(
            Guid methodologyId, Guid contentSectionId, Dictionary<Guid, int> newBlocksOrder)
        {
            return await _contentService
                .ReorderContentBlocks(methodologyId, contentSectionId, newBlocksOrder)
                .HandleFailuresOrOk();
        }

        [HttpPost("methodology/{methodologyId}/content/section/{contentSectionId}/blocks/add")]
        public async Task<ActionResult<IContentBlockViewModel>> AddContentBlock(
            Guid methodologyId, Guid contentSectionId, ContentBlockAddRequest request)
        {
            return await _contentService
                .AddContentBlock(methodologyId, contentSectionId, request)
                .HandleFailuresOrOk();
        }

        [HttpDelete("methodology/{methodologyId}/content/section/{contentSectionId}/block/{contentBlockId}")]
        public async Task<ActionResult<List<IContentBlockViewModel>>> RemoveContentBlock(
            Guid methodologyId, Guid contentSectionId, Guid contentBlockId)
        {
            return await _contentService
                .RemoveContentBlock(methodologyId, contentSectionId, contentBlockId)
                .HandleFailuresOrOk();
        }

        [HttpPut("methodology/{methodologyId}/content/section/{contentSectionId}/block/{contentBlockId}")]
        public async Task<ActionResult<IContentBlockViewModel>> UpdateTextBasedContentBlock(
            Guid methodologyId, Guid contentSectionId, Guid contentBlockId, ContentBlockUpdateRequest request)
        {
            return await _contentService
                .UpdateTextBasedContentBlock(methodologyId, contentSectionId, contentBlockId, request)
                .HandleFailuresOrOk();
        }
    }
}

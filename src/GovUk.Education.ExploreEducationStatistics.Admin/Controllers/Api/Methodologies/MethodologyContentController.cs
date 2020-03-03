using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using GovUk.Education.ExploreEducationStatistics.Admin.Controllers.Api.ManageContent;
using GovUk.Education.ExploreEducationStatistics.Admin.Models.Api;
using GovUk.Education.ExploreEducationStatistics.Admin.Services.Interfaces.Methodologies;
using GovUk.Education.ExploreEducationStatistics.Admin.Services.Methodologies;
using GovUk.Education.ExploreEducationStatistics.Common.Extensions;
using GovUk.Education.ExploreEducationStatistics.Content.Model;
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
        public async Task<ActionResult<List<ManageMethodologyContentViewModel>>> GetContent(
            Guid methodologyId)
        {
            return await _contentService
                .GetContentAsync(methodologyId)
                .HandleFailuresOr(Ok);
        }

        [HttpGet("methodology/{methodologyId}/content/sections")]
        public async Task<ActionResult<List<ContentSectionViewModel>>> GetContentSections(
            Guid methodologyId,
            [FromQuery] MethodologyContentService.ContentListType type)
        {
            return await _contentService
                .GetContentSectionsAsync(methodologyId, type)
                .HandleFailuresOr(Ok);
        }
        
        [HttpPut("methodology/{methodologyId}/content/sections/order")]
        public async Task<ActionResult<List<ContentSectionViewModel>>> ReorderSections(
            Guid methodologyId, Dictionary<Guid, int> newSectionOrder)
        {
            return await _contentService
                .ReorderContentSectionsAsync(methodologyId, newSectionOrder)
                .HandleFailuresOr(Ok);
        }

        [HttpPost("methodology/{methodologyId}/content/sections/add")]
        public async Task<ActionResult<ContentSectionViewModel>> AddContentSection(
            Guid methodologyId, 
            [FromQuery] MethodologyContentService.ContentListType type, 
            AddContentSectionRequest? request = null)
        {
            return await _contentService
                .AddContentSectionAsync(methodologyId, request, type)
                .HandleFailuresOr(Ok);
        }

        [HttpPut("methodology/{methodologyId}/content/section/{contentSectionId}/heading")]
        public async Task<ActionResult<ContentSectionViewModel>> UpdateContentSectionHeading(
            Guid methodologyId, Guid contentSectionId, UpdateContentSectionHeadingRequest request)
        {
            return await _contentService
                .UpdateContentSectionHeadingAsync(methodologyId, contentSectionId, request.Heading)
                .HandleFailuresOr(Ok);
        }

        [HttpDelete("methodology/{methodologyId}/content/section/{contentSectionId}")]
        public async Task<ActionResult<List<ContentSectionViewModel>>> RemoveContentSection(
            Guid methodologyId, Guid contentSectionId)
        {
            return await _contentService
                .RemoveContentSectionAsync(methodologyId, contentSectionId)
                .HandleFailuresOr(Ok);
        }

        [HttpGet("methodology/{methodologyId}/content/section/{contentSectionId}")]
        public async Task<ActionResult<ContentSectionViewModel>> GetContentSection(Guid methodologyId, Guid contentSectionId)
        {
            return await _contentService
                .GetContentSectionAsync(methodologyId, contentSectionId)
                .HandleFailuresOr(Ok);
        }
        
        [HttpPut("methodology/{methodologyId}/content/section/{contentSectionId}/blocks/order")]
        public async Task<ActionResult<List<IContentBlock>>> ReorderContentBlocks(
            Guid methodologyId, Guid contentSectionId, Dictionary<Guid, int> newBlocksOrder)
        {
            return await _contentService
                .ReorderContentBlocksAsync(methodologyId, contentSectionId, newBlocksOrder)
                .HandleFailuresOr(Ok);
        }

        [HttpPost("methodology/{methodologyId}/content/section/{contentSectionId}/blocks/add")]
        public async Task<ActionResult<IContentBlock>> AddContentBlock(
            Guid methodologyId, Guid contentSectionId, AddContentBlockRequest request)
        {
            return await _contentService
                .AddContentBlockAsync(methodologyId, contentSectionId, request)
                .HandleFailuresOr(Ok);
        }

        [HttpDelete("methodology/{methodologyId}/content/section/{contentSectionId}/block/{contentBlockId}")]
        public async Task<ActionResult<List<IContentBlock>>> RemoveContentBlock(
            Guid methodologyId, Guid contentSectionId, Guid contentBlockId)
        {
            return await _contentService
                .RemoveContentBlockAsync(methodologyId, contentSectionId, contentBlockId)
                .HandleFailuresOr(Ok);
        }

        [HttpPut("methodology/{methodologyId}/content/section/{contentSectionId}/block/{contentBlockId}")]
        public async Task<ActionResult<IContentBlock>> UpdateTextBasedContentBlock(
            Guid methodologyId, Guid contentSectionId, Guid contentBlockId, UpdateTextBasedContentBlockRequest request)
        {
            return await _contentService
                .UpdateTextBasedContentBlockAsync(methodologyId, contentSectionId, contentBlockId, request)
                .HandleFailuresOr(Ok);
        }
    }
}
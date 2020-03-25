using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using GovUk.Education.ExploreEducationStatistics.Admin.Models.Api;
using GovUk.Education.ExploreEducationStatistics.Admin.Services.Interfaces.ManageContent;
using GovUk.Education.ExploreEducationStatistics.Common.Extensions;
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
        public async Task<ActionResult<List<ContentSectionViewModel>>> GetContentSections(Guid releaseId)
        {
            return await _contentService
                .GetContentSectionsAsync(releaseId)
                .HandleFailuresOr(Ok);
        }
        
        [HttpPut("release/{releaseId}/content/sections/order")]
        public async Task<ActionResult<List<ContentSectionViewModel>>> ReorderSections(
            Guid releaseId, Dictionary<Guid, int> newSectionOrder)
        {
            return await _contentService
                .ReorderContentSectionsAsync(releaseId, newSectionOrder)
                .HandleFailuresOr(Ok);
        }

        [HttpPost("release/{releaseId}/content/sections/add")]
        public async Task<ActionResult<ContentSectionViewModel>> AddContentSection(
            Guid releaseId, AddContentSectionRequest? request = null)
        {
            return await _contentService
                .AddContentSectionAsync(releaseId, request)
                .HandleFailuresOr(Ok);
        }

        [HttpPut("release/{releaseId}/content/section/{contentSectionId}/heading")]
        public async Task<ActionResult<ContentSectionViewModel>> UpdateContentSectionHeading(
            Guid releaseId, Guid contentSectionId, UpdateContentSectionHeadingRequest request)
        {
            return await _contentService
                .UpdateContentSectionHeadingAsync(releaseId, contentSectionId, request.Heading)
                .HandleFailuresOr(Ok);
        }

        [HttpDelete("release/{releaseId}/content/section/{contentSectionId}")]
        public async Task<ActionResult<List<ContentSectionViewModel>>> RemoveContentSection(
            Guid releaseId, Guid contentSectionId)
        {
            return await _contentService
                .RemoveContentSectionAsync(releaseId, contentSectionId)
                .HandleFailuresOr(Ok);
        }

        [HttpGet("release/{releaseId}/content/section/{contentSectionId}")]
        public async Task<ActionResult<ContentSectionViewModel>> GetContentSection(Guid releaseId, Guid contentSectionId)
        {
            return await _contentService
                .GetContentSectionAsync(releaseId, contentSectionId)
                .HandleFailuresOr(Ok);
        }
        
        [HttpPut("release/{releaseId}/content/section/{contentSectionId}/blocks/order")]
        public async Task<ActionResult<List<IContentBlock>>> ReorderContentBlocks(
            Guid releaseId, Guid contentSectionId, Dictionary<Guid, int> newBlocksOrder)
        {
            return await _contentService
                .ReorderContentBlocksAsync(releaseId, contentSectionId, newBlocksOrder)
                .HandleFailuresOr(Ok);
        }

        [HttpPost("release/{releaseId}/content/section/{contentSectionId}/blocks/add")]
        public async Task<ActionResult<IContentBlock>> AddContentBlock(
            Guid releaseId, Guid contentSectionId, AddContentBlockRequest request)
        {
            return await _contentService
                .AddContentBlockAsync(releaseId, contentSectionId, request)
                .HandleFailuresOr(Ok);
        }

        [HttpDelete("release/{releaseId}/content/section/{contentSectionId}/block/{contentBlockId}")]
        public async Task<ActionResult<List<IContentBlock>>> RemoveContentBlock(
            Guid releaseId, Guid contentSectionId, Guid contentBlockId)
        {
            return await _contentService
                .RemoveContentBlockAsync(releaseId, contentSectionId, contentBlockId)
                .HandleFailuresOr(Ok);
        }

        [HttpPut("release/{releaseId}/content/section/{contentSectionId}/data-block/{contentBlockId}")]
        public async Task<ActionResult<IContentBlock>> UpdateDataBlock(
            Guid releaseId, Guid contentSectionId, Guid contentBlockId, UpdateDataBlockRequest request)
        {
            return await _contentService
                .UpdateDataBlockAsync(releaseId, contentSectionId, contentBlockId, request)
                .HandleFailuresOr(Ok);
        }

        [HttpPut("release/{releaseId}/content/section/{contentSectionId}/block/{contentBlockId}")]
        public async Task<ActionResult<IContentBlock>> UpdateTextBasedContentBlock(
            Guid releaseId, Guid contentSectionId, Guid contentBlockId, UpdateTextBasedContentBlockRequest request)
        {
            return await _contentService
                .UpdateTextBasedContentBlockAsync(releaseId, contentSectionId, contentBlockId, request)
                .HandleFailuresOr(Ok);
        }

        [HttpGet("release/{releaseId}/content/available-datablocks")]
        public async Task<ActionResult<List<DataBlock>>> GetAvailableDataBlocks(Guid releaseId)
        {
            return await _contentService
                .GetUnattachedContentBlocksAsync<DataBlock>(releaseId)
                .HandleFailuresOr(Ok);
        }

        [HttpPost("release/{releaseId}/content/section/{contentSectionId}/blocks/attach")]
        public async Task<ActionResult<IContentBlock>> AddContentBlock(
            Guid releaseId, Guid contentSectionId, AttachContentBlockRequest request)
        {
            return await _contentService
                .AttachContentBlockAsync(releaseId, contentSectionId, request)
                .HandleFailuresOr(Ok);
        }

        [HttpGet("release/{releaseId}/content/section/{contentSectionId}/block/{contentBlockId}/comments")]
        public async Task<ActionResult<List<CommentViewModel>>> GetComments(
            Guid releaseId, Guid contentSectionId, Guid contentBlockId)
        {
            return await _contentService
                .GetCommentsAsync(releaseId, contentSectionId, contentBlockId)
                .HandleFailuresOr(Ok);
        }
        
        
        [HttpPost("release/{releaseId}/content/section/{contentSectionId}/block/{contentBlockId}/comments/add")]
        public async Task<ActionResult<CommentViewModel>> AddComment(
            Guid releaseId, Guid contentSectionId, Guid contentBlockId, AddCommentRequest comment)
        {
            return await _contentService
                .AddCommentAsync(releaseId, contentSectionId, contentBlockId, comment)
                .HandleFailuresOr(Ok);
        }
        
                
        [HttpPut("release/{releaseId}/content/section/{contentSectionId}/block/{contentBlockId}/comment/{commentId}")]
        public async Task<ActionResult<CommentViewModel>> AddComment(
            Guid releaseId, Guid contentSectionId, Guid contentBlockId, Guid commentId, UpdateCommentRequest comment)
        {
            return await _contentService
                .UpdateCommentAsync(releaseId, contentSectionId, contentBlockId, commentId, comment)
                .HandleFailuresOr(Ok);
        }
        
        [HttpDelete("release/{releaseId}/content/section/{contentSectionId}/block/{contentBlockId}/comment/{commentId}")]
        public async Task<ActionResult<CommentViewModel>> DeleteComment(
            Guid releaseId, Guid contentSectionId, Guid contentBlockId, Guid commentId)
        {
            return await _contentService
                .DeleteCommentAsync(releaseId, contentSectionId, contentBlockId, commentId)
                .HandleFailuresOr(Ok);
        }
    }
}
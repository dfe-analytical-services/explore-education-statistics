#nullable enable
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
    public class CommentController : ControllerBase
    {
        private readonly ICommentService _commentService;

        public CommentController(ICommentService commentService)
        {
            _commentService = commentService;
        }

        [HttpGet("release/{releaseId}/content/section/{contentSectionId}/block/{contentBlockId}/comments")]
        public async Task<ActionResult<List<CommentViewModel>>> GetComments(
            Guid releaseId, Guid contentSectionId, Guid contentBlockId)
        {
            return await _commentService
                .GetComments(releaseId, contentSectionId, contentBlockId)
                .HandleFailuresOrOk();
        }

        [HttpPost("release/{releaseId}/content/section/{contentSectionId}/block/{contentBlockId}/comments/add")]
        public async Task<ActionResult<CommentViewModel>> AddComment(
            Guid releaseId, Guid contentSectionId, Guid contentBlockId, CommentSaveRequest saveRequest)
        {
            return await _commentService
                .AddComment(releaseId, contentSectionId, contentBlockId, saveRequest)
                .HandleFailuresOrOk();
        }

        [HttpPut("comment/{commentId}")]
        public async Task<ActionResult<CommentViewModel>> UpdateComment(Guid commentId, 
            CommentSaveRequest saveRequest)
        {
            if (saveRequest.SetResolved.HasValue)
            {
                return await _commentService
                    .SetResolved(commentId, saveRequest.SetResolved.Value)
                    .HandleFailuresOrOk();
            }

            return await _commentService
                .UpdateComment(commentId, saveRequest)
                .HandleFailuresOrOk();
        }

        [HttpDelete("comment/{commentId}")]
        public async Task<ActionResult> DeleteComment(Guid commentId)
        {
            return await _commentService
                .DeleteComment(commentId)
                .HandleFailuresOrNoContent();
        }
    }
}

#nullable enable
using GovUk.Education.ExploreEducationStatistics.Admin.Services.Interfaces.ManageContent;
using GovUk.Education.ExploreEducationStatistics.Admin.ViewModels;
using GovUk.Education.ExploreEducationStatistics.Admin.ViewModels.ManageContent;
using GovUk.Education.ExploreEducationStatistics.Common.Extensions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace GovUk.Education.ExploreEducationStatistics.Admin.Controllers.Api.ManageContent;

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

    [HttpGet(
        "release/{releaseVersionId:guid}/content/section/{contentSectionId:guid}/block/{contentBlockId:guid}/comments"
    )]
    public async Task<ActionResult<List<CommentViewModel>>> GetComments(
        Guid releaseVersionId,
        Guid contentSectionId,
        Guid contentBlockId
    )
    {
        return await _commentService
            .GetComments(
                releaseVersionId: releaseVersionId,
                contentSectionId: contentSectionId,
                contentBlockId: contentBlockId
            )
            .HandleFailuresOrOk();
    }

    [HttpPost(
        "release/{releaseVersionId:guid}/content/section/{contentSectionId:guid}/block/{contentBlockId:guid}/comments/add"
    )]
    public async Task<ActionResult<CommentViewModel>> AddComment(
        Guid releaseVersionId,
        Guid contentSectionId,
        Guid contentBlockId,
        CommentSaveRequest saveRequest
    )
    {
        return await _commentService
            .AddComment(
                releaseVersionId: releaseVersionId,
                contentSectionId: contentSectionId,
                contentBlockId: contentBlockId,
                saveRequest
            )
            .HandleFailuresOrOk();
    }

    [HttpPut("comment/{commentId:guid}")]
    public async Task<ActionResult<CommentViewModel>> UpdateComment(Guid commentId, CommentSaveRequest saveRequest)
    {
        if (saveRequest.SetResolved.HasValue)
        {
            return await _commentService.SetResolved(commentId, saveRequest.SetResolved.Value).HandleFailuresOrOk();
        }

        return await _commentService.UpdateComment(commentId, saveRequest).HandleFailuresOrOk();
    }

    [HttpDelete("comment/{commentId:guid}")]
    public async Task<ActionResult> DeleteComment(Guid commentId)
    {
        return await _commentService.DeleteComment(commentId).HandleFailuresOrNoContent();
    }
}

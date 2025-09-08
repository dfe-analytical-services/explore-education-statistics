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
public class ContentController : ControllerBase
{
    private readonly IContentService _contentService;

    public ContentController(IContentService contentService)
    {
        _contentService = contentService;
    }

    [HttpPut("release/{releaseVersionId:guid}/content/sections/order")]
    public async Task<ActionResult<List<ContentSectionViewModel>>> ReorderSections(
        Guid releaseVersionId,
        Dictionary<Guid, int> newSectionOrder
    )
    {
        return await _contentService.ReorderContentSections(releaseVersionId, newSectionOrder).HandleFailuresOrOk();
    }

    [HttpPost("release/{releaseVersionId:guid}/content/sections/add")]
    public async Task<ActionResult<ContentSectionViewModel>> AddContentSection(
        Guid releaseVersionId,
        ContentSectionAddRequest request = null
    )
    {
        return await _contentService.AddContentSectionAsync(releaseVersionId, request).HandleFailuresOrOk();
    }

    [HttpPut("release/{releaseVersionId:guid}/content/section/{contentSectionId:guid}/heading")]
    public async Task<ActionResult<ContentSectionViewModel>> UpdateContentSectionHeading(
        Guid releaseVersionId,
        Guid contentSectionId,
        ContentSectionHeadingUpdateRequest request
    )
    {
        return await _contentService
            .UpdateContentSectionHeading(
                releaseVersionId: releaseVersionId,
                contentSectionId: contentSectionId,
                request.Heading
            )
            .HandleFailuresOrOk();
    }

    [HttpDelete("release/{releaseVersionId:guid}/content/section/{contentSectionId:guid}")]
    public async Task<ActionResult<List<ContentSectionViewModel>>> RemoveContentSection(
        Guid releaseVersionId,
        Guid contentSectionId
    )
    {
        return await _contentService
            .RemoveContentSection(releaseVersionId: releaseVersionId, contentSectionId: contentSectionId)
            .HandleFailuresOrOk();
    }

    [HttpPut("release/{releaseVersionId:guid}/content/section/{contentSectionId:guid}/blocks/order")]
    public async Task<ActionResult<List<IContentBlockViewModel>>> ReorderContentBlocks(
        Guid releaseVersionId,
        Guid contentSectionId,
        Dictionary<Guid, int> newBlocksOrder
    )
    {
        return await _contentService
            .ReorderContentBlocks(
                releaseVersionId: releaseVersionId,
                contentSectionId: contentSectionId,
                newBlocksOrder
            )
            .HandleFailuresOrOk();
    }

    [HttpPost("release/{releaseVersionId:guid}/content/section/{contentSectionId:guid}/blocks/add")]
    public async Task<ActionResult<IContentBlockViewModel>> AddContentBlock(
        Guid releaseVersionId,
        Guid contentSectionId,
        ContentBlockAddRequest request
    )
    {
        return await _contentService
            .AddContentBlock(releaseVersionId: releaseVersionId, contentSectionId: contentSectionId, request)
            .HandleFailuresOrOk();
    }

    [HttpDelete("release/{releaseVersionId:guid}/content/section/{contentSectionId:guid}/block/{contentBlockId:guid}")]
    public async Task<ActionResult<List<IContentBlockViewModel>>> RemoveContentBlock(
        Guid releaseVersionId,
        Guid contentSectionId,
        Guid contentBlockId
    )
    {
        return await _contentService
            .RemoveContentBlock(releaseVersionId: releaseVersionId, contentSectionId: contentSectionId, contentBlockId)
            .HandleFailuresOrOk();
    }

    [HttpPut("release/{releaseVersionId:guid}/content/section/{contentSectionId:guid}/block/{contentBlockId:guid}")]
    public async Task<ActionResult<IContentBlockViewModel>> UpdateTextBasedContentBlock(
        Guid releaseVersionId,
        Guid contentSectionId,
        Guid contentBlockId,
        ContentBlockUpdateRequest request
    )
    {
        return await _contentService
            .UpdateTextBasedContentBlock(
                releaseVersionId: releaseVersionId,
                contentSectionId: contentSectionId,
                contentBlockId: contentBlockId,
                request
            )
            .HandleFailuresOrOk();
    }

    [HttpPost("release/{releaseVersionId:guid}/content/section/{contentSectionId:guid}/blocks/attach")]
    public async Task<ActionResult<DataBlockViewModel>> AttachDataBlock(
        Guid releaseVersionId,
        Guid contentSectionId,
        DataBlockAttachRequest request
    )
    {
        return await _contentService
            .AttachDataBlock(releaseVersionId: releaseVersionId, contentSectionId: contentSectionId, request)
            .HandleFailuresOrOk();
    }
}

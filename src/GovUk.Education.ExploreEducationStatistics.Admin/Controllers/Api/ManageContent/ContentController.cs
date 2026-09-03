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
public class ContentController(IContentService contentService) : ControllerBase
{
    [HttpPut("release/{releaseVersionId:guid}/content/sections/order")]
    public async Task<ActionResult<List<ContentSectionViewModel>>> ReorderSections(
        Guid releaseVersionId,
        Dictionary<Guid, int> newSectionOrder
    ) => await contentService.ReorderContentSections(releaseVersionId, newSectionOrder).HandleFailuresOrOk();

    [HttpPost("release/{releaseVersionId:guid}/content/sections/add")]
    public async Task<ActionResult<ContentSectionViewModel>> AddGenericContentSection(
        Guid releaseVersionId,
        ContentSectionAddRequest request = null
    ) => await contentService.AddGenericContentSection(releaseVersionId, request).HandleFailuresOrOk();

    [HttpPut("release/{releaseVersionId:guid}/content/section/{contentSectionId:guid}/heading")]
    public async Task<ActionResult<ContentSectionViewModel>> UpdateContentSectionHeading(
        Guid releaseVersionId,
        Guid contentSectionId,
        ContentSectionHeadingUpdateRequest request
    ) =>
        await contentService
            .UpdateContentSectionHeading(
                releaseVersionId: releaseVersionId,
                contentSectionId: contentSectionId,
                request.Heading
            )
            .HandleFailuresOrOk();

    [HttpDelete("release/{releaseVersionId:guid}/content/section/{contentSectionId:guid}")]
    public async Task<ActionResult<List<ContentSectionViewModel>>> RemoveGenericContentSection(
        Guid releaseVersionId,
        Guid contentSectionId
    ) =>
        await contentService
            .RemoveGenericContentSection(releaseVersionId: releaseVersionId, contentSectionId: contentSectionId)
            .HandleFailuresOrOk();

    [HttpPut("release/{releaseVersionId:guid}/content/section/{contentSectionId:guid}/blocks/order")]
    public async Task<ActionResult<List<IContentBlockViewModel>>> ReorderContentBlocks(
        Guid releaseVersionId,
        Guid contentSectionId,
        Dictionary<Guid, int> newBlocksOrder
    ) =>
        await contentService
            .ReorderContentBlocks(
                releaseVersionId: releaseVersionId,
                contentSectionId: contentSectionId,
                newBlocksOrder
            )
            .HandleFailuresOrOk();

    [HttpPost("release/{releaseVersionId:guid}/content/section/{contentSectionId:guid}/blocks/add")]
    public async Task<ActionResult<IContentBlockViewModel>> AddContentBlock(
        Guid releaseVersionId,
        Guid contentSectionId,
        ContentBlockAddRequest request
    ) =>
        await contentService
            .AddContentBlock(releaseVersionId: releaseVersionId, contentSectionId: contentSectionId, request)
            .HandleFailuresOrOk();

    [HttpDelete("release/{releaseVersionId:guid}/content/section/{contentSectionId:guid}/block/{contentBlockId:guid}")]
    public async Task<ActionResult<List<IContentBlockViewModel>>> RemoveContentBlock(
        Guid releaseVersionId,
        Guid contentSectionId,
        Guid contentBlockId
    ) =>
        await contentService
            .RemoveContentBlock(releaseVersionId: releaseVersionId, contentSectionId: contentSectionId, contentBlockId)
            .HandleFailuresOrOk();

    [HttpPut("release/{releaseVersionId:guid}/content/section/{contentSectionId:guid}/block/{contentBlockId:guid}")]
    public async Task<ActionResult<IContentBlockViewModel>> UpdateTextBasedContentBlock(
        Guid releaseVersionId,
        Guid contentSectionId,
        Guid contentBlockId,
        ContentBlockUpdateRequest request
    ) =>
        await contentService
            .UpdateTextBasedContentBlock(
                releaseVersionId: releaseVersionId,
                contentSectionId: contentSectionId,
                contentBlockId: contentBlockId,
                request
            )
            .HandleFailuresOrOk();

    [HttpPost("release/{releaseVersionId:guid}/content/section/{contentSectionId:guid}/blocks/attach")]
    public async Task<ActionResult<DataBlockViewModel>> AttachDataBlock(
        Guid releaseVersionId,
        Guid contentSectionId,
        DataBlockAttachRequest request
    ) =>
        await contentService
            .AttachDataBlock(releaseVersionId: releaseVersionId, contentSectionId: contentSectionId, request)
            .HandleFailuresOrOk();
}

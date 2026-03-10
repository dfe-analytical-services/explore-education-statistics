#nullable enable
using GovUk.Education.ExploreEducationStatistics.Admin.Models;
using GovUk.Education.ExploreEducationStatistics.Admin.Requests;
using GovUk.Education.ExploreEducationStatistics.Admin.Services.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Admin.ViewModels;
using GovUk.Education.ExploreEducationStatistics.Common.Extensions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace GovUk.Education.ExploreEducationStatistics.Admin.Controllers.Api;

[Route("api")]
[ApiController]
[Authorize(Roles = GlobalRoles.RoleNames.BauUser)]
public class EducationInNumbersContentController(IEducationInNumbersContentService einContentService) : ControllerBase
{
    [HttpGet("education-in-numbers/{pageId:guid}/content")]
    public async Task<ActionResult<EinContentViewModel>> GetPageContent(
        [FromRoute] Guid pageId,
        CancellationToken cancellationToken
    )
    {
        return await einContentService.GetPageContent(pageId, cancellationToken).HandleFailuresOrOk();
    }

    [HttpPost("education-in-numbers/{pageId:guid}/content/sections/add")]
    public async Task<ActionResult<EinContentSectionViewModel>> AddSection(
        [FromRoute] Guid pageId,
        [FromBody] EinContentSectionAddRequest request,
        CancellationToken cancellationToken
    )
    {
        return await einContentService.AddSection(pageId, request.Order, cancellationToken).HandleFailuresOrOk();
    }

    [HttpPut("education-in-numbers/{pageId:guid}/content/section/{sectionId:guid}/heading")]
    public async Task<ActionResult<EinContentSectionViewModel>> UpdateSectionHeading(
        [FromRoute] Guid pageId,
        [FromRoute] Guid sectionId,
        [FromBody] EinContentSectionUpdateHeadingRequest request,
        CancellationToken cancellationToken
    )
    {
        return await einContentService
            .UpdateSectionHeading(pageId, sectionId, request.Heading, cancellationToken)
            .HandleFailuresOrOk();
    }

    [HttpPut("education-in-numbers/{pageId:guid}/content/sections/order")]
    public async Task<ActionResult<List<EinContentSectionViewModel>>> UpdatePageSectionOrder(
        [FromRoute] Guid pageId,
        [FromBody] List<Guid> order,
        CancellationToken cancellationToken
    )
    {
        return await einContentService.ReorderSections(pageId, order, cancellationToken).HandleFailuresOrOk();
    }

    [HttpDelete("education-in-numbers/{pageId:guid}/content/section/{sectionId:guid}")]
    public async Task<ActionResult<List<EinContentSectionViewModel>>> DeleteSection(
        [FromRoute] Guid pageId,
        [FromRoute] Guid sectionId,
        CancellationToken cancellationToken
    )
    {
        return await einContentService.DeleteSection(pageId, sectionId, cancellationToken).HandleFailuresOrOk();
    }

    [HttpPost("education-in-numbers/{pageId:guid}/content/section/{sectionId:guid}/blocks/add")]
    public async Task<ActionResult<EinContentBlockViewModel>> AddBlock(
        [FromRoute] Guid pageId,
        [FromRoute] Guid sectionId,
        [FromBody] EinContentBlockAddRequest request,
        CancellationToken cancellationToken
    )
    {
        return await einContentService
            .AddBlock(pageId, sectionId, request.Type, request.Order, cancellationToken)
            .HandleFailuresOrOk();
    }

    [HttpPut("education-in-numbers/{pageId:guid}/content/section/{sectionId:guid}/block/{blockId:guid}/html")]
    public async Task<ActionResult<EinContentBlockViewModel>> UpdateHtmlBlock(
        [FromRoute] Guid pageId,
        [FromRoute] Guid sectionId,
        [FromRoute] Guid blockId,
        [FromBody] EinHtmlBlockUpdateRequest request,
        CancellationToken cancellationToken
    )
    {
        return await einContentService
            .UpdateHtmlBlock(pageId, sectionId, blockId, request, cancellationToken)
            .HandleFailuresOrOk();
    }

    [HttpPut("education-in-numbers/{pageId:guid}/content/section/{sectionId:guid}/block/{blockId:guid}/tile-group")]
    public async Task<ActionResult<EinContentBlockViewModel>> UpdateTileGroupBlock(
        [FromRoute] Guid pageId,
        [FromRoute] Guid sectionId,
        [FromRoute] Guid blockId,
        [FromBody] EinTileGroupBlockUpdateRequest request,
        CancellationToken cancellationToken
    )
    {
        return await einContentService
            .UpdateTileGroupBlock(pageId, sectionId, blockId, request, cancellationToken)
            .HandleFailuresOrOk();
    }

    [HttpPut("education-in-numbers/{pageId:guid}/content/section/{sectionId:guid}/blocks/order")]
    public async Task<ActionResult<List<EinContentBlockViewModel>>> ReorderBlocks(
        [FromRoute] Guid pageId,
        [FromRoute] Guid sectionId,
        [FromBody] List<Guid> order,
        CancellationToken cancellationToken
    )
    {
        return await einContentService.ReorderBlocks(pageId, sectionId, order, cancellationToken).HandleFailuresOrOk();
    }

    [HttpDelete("education-in-numbers/{pageId:guid}/content/section/{sectionId:guid}/block/{blockId:guid}")]
    public async Task<ActionResult> DeleteBlock(
        [FromRoute] Guid pageId,
        [FromRoute] Guid sectionId,
        [FromRoute] Guid blockId,
        CancellationToken cancellationToken
    )
    {
        return await einContentService
            .DeleteBlock(pageId, sectionId, blockId, cancellationToken)
            .HandleFailuresOrNoContent();
    }

    [HttpPost("education-in-numbers/{pageId:guid}/content/block/{blockId:guid}/tiles/add")]
    public async Task<ActionResult<EinTileViewModel>> AddTile(
        [FromRoute] Guid pageId,
        [FromRoute] Guid blockId,
        [FromBody] EinTileAddRequest request,
        CancellationToken cancellationToken
    )
    {
        return await einContentService
            .AddTile(pageId, blockId, request.Type, request.Order, cancellationToken)
            .HandleFailuresOrOk();
    }

    [HttpPut("education-in-numbers/{pageId:guid}/content/tile/{tileId:guid}/free-text-stat")]
    public async Task<ActionResult<EinTileViewModel>> UpdateFreeTextStatTile(
        [FromRoute] Guid pageId,
        [FromRoute] Guid tileId,
        [FromBody] EinFreeTextStatTileUpdateRequest request,
        CancellationToken cancellationToken
    )
    {
        return await einContentService
            .UpdateFreeTextStatTile(pageId, tileId, request, cancellationToken)
            .HandleFailuresOrOk();
    }

    [HttpPut("education-in-numbers/{pageId:guid}/content/tile/{tileId:guid}/api-stat")]
    public async Task<ActionResult<EinTileViewModel>> UpdateApiQueryStatTile(
        [FromRoute] Guid pageId,
        [FromRoute] Guid tileId,
        [FromBody] EinApiQueryStatTileUpdateRequest request,
        CancellationToken cancellationToken
    )
    {
        return await einContentService
            .UpdateApiQueryStatTile(pageId, tileId, request, cancellationToken)
            .HandleFailuresOrOk();
    }

    [HttpPut("education-in-numbers/{pageId:guid}/content/block/{blockId:guid}/tiles/order")]
    public async Task<ActionResult<List<EinTileViewModel>>> ReorderTiles(
        [FromRoute] Guid pageId,
        [FromRoute] Guid blockId,
        [FromBody] List<Guid> order,
        CancellationToken cancellationToken
    )
    {
        return await einContentService.ReorderTiles(pageId, blockId, order, cancellationToken).HandleFailuresOrOk();
    }

    [HttpDelete("education-in-numbers/{pageId:guid}/content/block/{blockId:guid}/tile/{tileId:guid}")]
    public async Task<ActionResult> DeleteTile(
        [FromRoute] Guid pageId,
        [FromRoute] Guid blockId,
        [FromRoute] Guid tileId,
        CancellationToken cancellationToken
    )
    {
        return await einContentService
            .DeleteTile(pageId, blockId, tileId, cancellationToken)
            .HandleFailuresOrNoContent();
    }
}

#nullable enable
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
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
public class EducationInNumbersContentController(
    IEducationInNumbersContentService einContentService) : ControllerBase
{
    [HttpGet("education-in-numbers/{pageId:guid}/content")]
    public async Task<ActionResult<EinContentViewModel>> GetPageContent(
        [FromRoute] Guid pageId)
    {
        return await einContentService.GetPageContent(pageId)
            .HandleFailuresOrOk();
    }

    [HttpPost("education-in-numbers/{pageId:guid}/content/sections/add")]
    public async Task<ActionResult<EinContentSectionViewModel>> AddSection(
        [FromRoute] Guid pageId,
        [FromBody] EinContentSectionAddRequest request)
    {
        return await einContentService.AddSection(pageId, request.Order)
            .HandleFailuresOrOk();
    }

    [HttpPut("education-in-numbers/{pageId:guid}/content/section/{sectionId:guid}/heading")]
    public async Task<ActionResult<EinContentSectionViewModel>> UpdateSectionHeading(
        [FromRoute] Guid pageId,
        [FromRoute] Guid sectionId,
        [FromBody] EinContentSectionUpdateHeadingRequest request)
    {
        return await einContentService.UpdateSectionHeading(pageId, sectionId, request.Heading)
            .HandleFailuresOrOk();
    }

    [HttpPut("education-in-numbers/{pageId:guid}/content/sections/order")]
    public async Task<ActionResult<List<EinContentSectionViewModel>>> UpdatePageSectionOrder(
        [FromRoute] Guid pageId,
        [FromBody] List<Guid> order)
    {
        return await einContentService.ReorderSections(pageId, order)
            .HandleFailuresOrOk();
    }

    [HttpDelete("education-in-numbers/{pageId:guid}/content/section/{sectionId:guid}")]
    public async Task<ActionResult<List<EinContentSectionViewModel>>> DeleteSection( // @MarkFix do we want to return a list of remaining sections here? Check the return types of other methods too
        [FromRoute] Guid pageId,
        [FromRoute] Guid sectionId)
    {
        return await einContentService.DeleteSection(pageId, sectionId)
            .HandleFailuresOrOk(); // @MarkFix is NoContent for Deletes normally?
    }

    [HttpPost("education-in-numbers/{pageId:guid}/content/section/{sectionId:guid}/blocks/add")]
    public async Task<ActionResult<EinContentBlockViewModel>> AddBlock(
        [FromRoute] Guid pageId,
        [FromRoute] Guid sectionId,
        [FromBody] EinContentBlockAddRequest request)
    {
        return await einContentService.AddBlock(pageId, sectionId, request.Type, request.Order)
            .HandleFailuresOrOk();
    }

    [HttpPut("education-in-numbers/{pageId:guid}/content/section/{sectionId:guid}/block/{blockId:guid}/html")]
    public async Task<ActionResult<EinContentBlockViewModel>> UpdateHtmlBlock(
        [FromRoute] Guid pageId,
        [FromRoute] Guid sectionId,
        [FromRoute] Guid blockId,
        [FromBody] EinHtmlBlockUpdateRequest request)
    {
        return await einContentService.UpdateHtmlBlock(pageId, sectionId, blockId, request)
            .HandleFailuresOrOk();
    }

    [HttpPut("education-in-numbers/{pageId:guid}/content/section/{sectionId:guid}/blocks/order")]
    public async Task<ActionResult<List<EinContentBlockViewModel>>> UpdateSectionBlocksOrder(
        [FromRoute] Guid pageId,
        [FromRoute] Guid sectionId,
        [FromBody] List<Guid> order)
    {
        return await einContentService.ReorderBlocks(pageId, sectionId, order)
            .HandleFailuresOrOk();
    }


    [HttpDelete("education-in-numbers/{pageId:guid}/content/section/{sectionId:guid}/block/{blockId:guid}")]
    public async Task<ActionResult<List<EinContentBlockViewModel>>> DeleteBlock( // @MarkFix at the time of writing, the frontend expects a list of sections to be returned
        [FromRoute] Guid pageId,
        [FromRoute] Guid sectionId,
        [FromRoute] Guid blockId)
    {
        return await einContentService.DeleteBlock(pageId, sectionId, blockId)
            .HandleFailuresOrOk(); // @MarkFix NoContent?
    }


}

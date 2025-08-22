#nullable enable
using System;
using System.Collections.Generic;
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
    public async Task<ActionResult<EducationInNumbersContentViewModel>> GetPageContent(
        [FromRoute] Guid pageId)
    {
        return await einContentService.GetPageContent(pageId)
            .HandleFailuresOrOk();
    }

    [HttpPost("education-in-numbers/{pageId:guid}/content/section/add")]
    public async Task<ActionResult<EinContentSectionViewModel>> AddSection(
        [FromRoute] Guid pageId,
        [FromBody] int order)
    {
        return await einContentService.AddSection(pageId, order)
            .HandleFailuresOrOk();
    }

    [HttpPut("education-in-numbers/{pageId:guid}/content/section/{sectionId:guid}/heading")]
    public async Task<ActionResult<EinContentSectionViewModel>> UpdateSectionHeading(
        [FromRoute] Guid pageId,
        [FromRoute] Guid sectionId,
        [FromBody] string heading)
    {
        return await einContentService.UpdateSectionHeading(pageId, sectionId, heading)
            .HandleFailuresOrOk();
    }

    [HttpPut("education-in-numbers/{pageId:guid}/content/sections/order")]
    public async Task<ActionResult<List<EinContentSectionViewModel>>> UpdatePageSectionOrder(
        [FromRoute] Guid pageId,
        [FromBody] Dictionary<Guid, int> newSectionOrder)
    {
        return new List<EinContentSectionViewModel>(); // @MarkFix
    }

    [HttpDelete("education-in-numbers/{pageId:guid}/content/section/{sectionId:guid}")]
    public async Task<ActionResult<List<EinContentSectionViewModel>>> DeleteSection(
        [FromRoute] Guid pageId,
        [FromRoute] Guid sectionId)
    {
        return new List<EinContentSectionViewModel>(); // @MarkFix
    }

    [HttpPost("education-in-numbers/{pageId:guid}/content/section/{sectionId:guid}/blocks/add")]
    public async Task<ActionResult<EinContentBlockViewModel>> AddBlock(
        [FromRoute] Guid pageId,
        [FromRoute] Guid sectionId,
        [FromBody] EinContentBlockAddRequest request)
    {
        return new EinContentBlockViewModel(); // @MarkFix frontend wants editableContentBlock?
    }

    [HttpPut("education-in-numbers/{pageId:guid}/content/section/{sectionId:guid}/block/{blockId:guid}")]
    public async Task<ActionResult<EinContentBlockViewModel>> UpdateBlock(
        [FromRoute] Guid pageId,
        [FromRoute] Guid sectionId,
        [FromRoute] Guid blockId,
        [FromBody] EinContentBlockUpdateRequest request)
    {
        return new EinContentBlockViewModel(); // @MarkFix
    }

    [HttpPut("education-in-numbers/{pageId:guid}/content/section/{sectionId:guid}/blocks/order")]
    public async Task<ActionResult<List<EinContentBlockViewModel>>> UpdateSectionBlocksOrder(
        [FromRoute] Guid pageId,
        [FromRoute] Guid sectionId,
        [FromBody] Dictionary<Guid, int> newSectionOrder)
    {
        return new List<EinContentBlockViewModel>(); // @MarkFix
    }


    [HttpDelete("education-in-numbers/{pageId:guid}/content/section/{sectionId:guid}/block/{blockId:guid}")]
    public async Task<ActionResult<List<EinContentSectionViewModel>>> DeleteSection(
        [FromRoute] Guid pageId,
        [FromRoute] Guid sectionId,
        [FromRoute] Guid blockId)
    {
        return new List<EinContentSectionViewModel>(); // @MarkFix
    }


}

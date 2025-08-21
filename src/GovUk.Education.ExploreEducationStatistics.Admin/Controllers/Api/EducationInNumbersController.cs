#nullable enable
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using GovUk.Education.ExploreEducationStatistics.Admin.Models;
using GovUk.Education.ExploreEducationStatistics.Admin.Requests;
using GovUk.Education.ExploreEducationStatistics.Admin.Services.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Admin.ViewModels;
using GovUk.Education.ExploreEducationStatistics.Common.Extensions;
using GovUk.Education.ExploreEducationStatistics.Common.Model;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace GovUk.Education.ExploreEducationStatistics.Admin.Controllers.Api;

[Route("api")]
[ApiController]
[Authorize(Roles = GlobalRoles.RoleNames.BauUser)]
public class EducationInNumbersController(
    IEducationInNumbersService einService) : ControllerBase
{
    [HttpGet("education-in-numbers/{id:guid}")]
    public async Task<ActionResult<EducationInNumbersSummaryViewModel>> GetLatestPage(
        [FromRoute] Guid id)
    {
        return await einService.GetPage(id)
            .HandleFailuresOrOk();
    }

    [HttpGet("education-in-numbers")]
    public async Task<ActionResult<List<EducationInNumbersSummaryWithPrevVersionViewModel>>> ListLatestPages()
    {
        return await einService.ListLatestPages()
            .HandleFailuresOrOk();
    }

    [HttpPost("education-in-numbers")]
    public async Task<ActionResult<EducationInNumbersSummaryViewModel>> CreateEducationInNumbersPage(
        [FromBody] CreateEducationInNumbersPageRequest request)
    {
        return await einService.CreatePage(request)
            .HandleFailuresOrOk();
    }

    [HttpPost("education-in-numbers/{id:Guid}/amendment")]
    public async Task<ActionResult<EducationInNumbersSummaryViewModel>> CreateAmendment(
        [FromRoute] Guid id)
    {
        return await einService.CreateAmendment(id)
            .HandleFailuresOrOk();
    }

    [HttpPut("education-in-numbers/{id:Guid}")]
    public async Task<ActionResult<EducationInNumbersSummaryViewModel>> UpdatePage(
        [FromRoute] Guid id,
        [FromBody] UpdateEducationInNumbersPageRequest request)
    {
        return await einService.UpdatePage(id, request)
            .HandleFailuresOrOk();
    }

    [HttpPatch("education-in-numbers/{id:Guid}/publish")]
    public async Task<ActionResult<EducationInNumbersSummaryViewModel>> PublishPage(
        [FromRoute] Guid id)
    {
        return await einService.PublishPage(id)
            .HandleFailuresOrOk();
    }

    [HttpPut("education-in-numbers/order")]
    public async Task<ActionResult<List<EducationInNumbersSummaryViewModel>>> Reorder(
        [FromBody] List<Guid> pageIds)
    {
        return await einService.Reorder(pageIds)
            .HandleFailuresOrOk();
    }

    [HttpDelete("education-in-numbers/{id:guid}")]
    public async Task<ActionResult<Unit>> Delete(
        [FromRoute] Guid id)
    {
        return await einService.Delete(id)
            .HandleFailuresOrOk();
    }
}

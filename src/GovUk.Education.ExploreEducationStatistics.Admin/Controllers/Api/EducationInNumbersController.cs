#nullable enable
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
public class EducationInNumbersController(IEducationInNumbersService einService) : ControllerBase
{
    [HttpGet("education-in-numbers/{pageVersionId:guid}")]
    public async Task<ActionResult<EinPageVersionSummaryViewModel>> GetPageVersion([FromRoute] Guid pageVersionId)
    {
        return await einService.GetPageVersion(pageVersionId).HandleFailuresOrOk();
    }

    [HttpGet("education-in-numbers")]
    public async Task<ActionResult<List<EinPageVersionSummaryWithPrevVersionViewModel>>> ListLatestPages()
    {
        return await einService.ListLatestPages().HandleFailuresOrOk();
    }

    [HttpPost("education-in-numbers")]
    public async Task<ActionResult<EinPageVersionSummaryViewModel>> CreateEinPage(
        [FromBody] CreateEducationInNumbersPageRequest request
    )
    {
        return await einService.CreatePage(request).HandleFailuresOrOk();
    }

    [HttpPost("education-in-numbers/{pageVersionId:Guid}/amendment")]
    public async Task<ActionResult<EinPageVersionSummaryViewModel>> CreateAmendment([FromRoute] Guid pageVersionId)
    {
        return await einService.CreateAmendment(pageVersionId).HandleFailuresOrOk();
    }

    [HttpPut("education-in-numbers/{pageVersionId:Guid}")]
    public async Task<ActionResult<EinPageVersionSummaryViewModel>> UpdatePage(
        [FromRoute] Guid pageVersionId,
        [FromBody] UpdateEducationInNumbersPageRequest request
    )
    {
        return await einService.UpdatePage(pageVersionId, request).HandleFailuresOrOk();
    }

    [HttpPatch("education-in-numbers/{pageVersionId:Guid}/publish")]
    public async Task<ActionResult<EinPageVersionSummaryViewModel>> PublishPage([FromRoute] Guid pageVersionId)
    {
        return await einService.PublishPage(pageVersionId).HandleFailuresOrOk();
    }

    [HttpPut("education-in-numbers/order")]
    public async Task<ActionResult<List<EinPageVersionSummaryViewModel>>> Reorder([FromBody] List<Guid> pageVersionIds)
    {
        return await einService.Reorder(pageVersionIds).HandleFailuresOrOk();
    }

    [HttpDelete("education-in-numbers/{pageVersionId:guid}")]
    public async Task<ActionResult<Unit>> Delete([FromRoute] Guid pageVersionId)
    {
        return await einService.Delete(pageVersionId).HandleFailuresOrOk();
    }

    [HttpDelete("education-in-numbers/full-delete/{slug}")]
    public async Task<ActionResult<Unit>> FullDelete([FromRoute] string slug)
    {
        return await einService.FullDelete(slug).HandleFailuresOrOk();
    }
}

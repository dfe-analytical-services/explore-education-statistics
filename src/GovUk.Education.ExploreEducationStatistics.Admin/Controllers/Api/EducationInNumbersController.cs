#nullable enable
using System.Collections.Generic;
using System.Threading.Tasks;
using GovUk.Education.ExploreEducationStatistics.Admin.Models;
using GovUk.Education.ExploreEducationStatistics.Admin.Services.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Admin.ViewModels;
using GovUk.Education.ExploreEducationStatistics.Common.Extensions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace GovUk.Education.ExploreEducationStatistics.Admin.Controllers.Api;

[Route("api")]
[ApiController]
[Authorize(Roles = GlobalRoles.RoleNames.BauUser)]
public class EducationInNumbersController(
    IEducationInNumbersService einService) : ControllerBase
{
    [HttpGet("education-in-numbers/{slug}/latest")]
    public async Task<ActionResult<EducationInNumbersPageViewModel>> GetLatestPage(
        [FromRoute] string? slug)
    {
        return await einService.GetPage(slug)
            .HandleFailuresOrOk();
    }

    [HttpGet("education-in-numbers/{slug}/published")]
    public async Task<ActionResult<EducationInNumbersPageViewModel>> GetLatestPublishedPage(
        [FromRoute] string? slug)
    {
        return await einService.GetPage(slug, published: true)
            .HandleFailuresOrOk();
    }

    [HttpGet("education-in-numbers")]
    public async Task<ActionResult<List<EducationInNumbersPageViewModel>>> ListLatestPages()
    {
        return await einService.ListLatestPages()
            .HandleFailuresOrOk();
    }
}

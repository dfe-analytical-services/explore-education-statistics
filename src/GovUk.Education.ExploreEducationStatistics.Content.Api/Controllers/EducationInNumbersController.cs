#nullable enable
using GovUk.Education.ExploreEducationStatistics.Common.Extensions;
using GovUk.Education.ExploreEducationStatistics.Content.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;
using static GovUk.Education.ExploreEducationStatistics.Content.ViewModels.EducationInNumbersViewModels;

namespace GovUk.Education.ExploreEducationStatistics.Content.Api.Controllers;

[Route("api")]
public class EducationInNumbersController(IEducationInNumbersService einService) : ControllerBase
{
    [HttpGet("education-in-numbers-nav")]
    public async Task<ActionResult<List<EinNavItemViewModel>>> List()
    {
        return await einService.ListEinPages().HandleFailuresOrOk();
    }

    [HttpGet("education-in-numbers")]
    [HttpGet("education-in-numbers/{slug}")]
    public async Task<ActionResult<EinPageViewModel>> GetEinPage(string? slug)
    {
        return await einService.GetEinPage(slug).HandleFailuresOrOk();
    }

    [HttpGet("education-in-numbers-sitemap-items")]
    public async Task<ActionResult<List<EinPageSitemapItemViewModel>>> ListSitemapItems()
    {
        return await einService.ListSitemapItems().HandleFailuresOrOk();
    }
}

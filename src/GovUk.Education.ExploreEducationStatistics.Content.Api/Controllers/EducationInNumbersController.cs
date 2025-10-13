#nullable enable
using GovUk.Education.ExploreEducationStatistics.Common.Extensions;
using GovUk.Education.ExploreEducationStatistics.Content.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;
using static GovUk.Education.ExploreEducationStatistics.Content.ViewModels.EducationInNumbersViewModels;

namespace GovUk.Education.ExploreEducationStatistics.Content.Api.Controllers;

[Route("api")]
public class EducationInNumbersController(IEducationInNumbersService einService) : ControllerBase
{
    [HttpGet("education-in-numbers/nav")]
    public async Task<ActionResult<List<EinNavItemViewModel>>> List(CancellationToken cancellationToken)
    {
        return await einService.ListEinPages(cancellationToken).HandleFailuresOrOk();
    }

    [HttpGet("education-in-numbers")]
    [HttpGet("education-in-numbers/pages/{slug}")]
    public async Task<ActionResult<EinPageViewModel>> GetEinPage(string? slug, CancellationToken cancellationToken)
    {
        return await einService.GetEinPage(slug, cancellationToken).HandleFailuresOrOk();
    }

    [HttpGet("education-in-numbers/sitemap-items")]
    public async Task<ActionResult<List<EinPageSitemapItemViewModel>>> ListSitemapItems(
        CancellationToken cancellationToken
    )
    {
        return await einService.ListSitemapItems(cancellationToken).HandleFailuresOrOk();
    }
}

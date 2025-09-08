#nullable enable
using GovUk.Education.ExploreEducationStatistics.Common.Extensions;
using GovUk.Education.ExploreEducationStatistics.Content.Services.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Content.ViewModels;
using Microsoft.AspNetCore.Mvc;

namespace GovUk.Education.ExploreEducationStatistics.Content.Api.Controllers;

[Route("api")]
public class EducationInNumbersController(IEducationInNumbersService einService) : ControllerBase
{
    [HttpGet("education-in-numbers-nav")]
    public async Task<ActionResult<List<EducationInNumbersViewModels.EinNavItemViewModel>>> List()
    {
        return await einService.ListEinPages().HandleFailuresOrOk();
    }

    [HttpGet("education-in-numbers")]
    [HttpGet("education-in-numbers/{slug}")]
    public async Task<ActionResult<EducationInNumbersViewModels.EinPageViewModel>> GetEinPage(string? slug)
    {
        return await einService.GetEinPage(slug).HandleFailuresOrOk();
    }
}

#nullable enable
using GovUk.Education.ExploreEducationStatistics.Common.Extensions;
using GovUk.Education.ExploreEducationStatistics.Content.Services.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Content.ViewModels;
using Microsoft.AspNetCore.Mvc;

namespace GovUk.Education.ExploreEducationStatistics.Content.Api.Controllers;

[Route("api")]
public class DataGuidanceController
{
    private readonly IDataGuidanceService _dataGuidanceService;

    public DataGuidanceController(IDataGuidanceService dataGuidanceService)
    {
        _dataGuidanceService = dataGuidanceService;
    }

    [HttpGet("publications/{publicationSlug}/releases/latest/data-guidance")]
    public async Task<ActionResult<DataGuidanceViewModel>> GetLatestReleaseDataGuidance(
        string publicationSlug
    )
    {
        return await _dataGuidanceService.GetDataGuidance(publicationSlug).HandleFailuresOrOk();
    }

    [HttpGet("publications/{publicationSlug}/releases/{releaseSlug}/data-guidance")]
    public async Task<ActionResult<DataGuidanceViewModel>> GetReleaseDataGuidance(
        string publicationSlug,
        string releaseSlug
    )
    {
        return await _dataGuidanceService
            .GetDataGuidance(publicationSlug, releaseSlug)
            .HandleFailuresOrOk();
    }
}

#nullable enable
using System;
using System.Threading.Tasks;
using GovUk.Education.ExploreEducationStatistics.Admin.Requests;
using GovUk.Education.ExploreEducationStatistics.Admin.Services.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Admin.ViewModels;
using GovUk.Education.ExploreEducationStatistics.Common.Extensions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace GovUk.Education.ExploreEducationStatistics.Admin.Controllers.Api;

[Route("api")]
[Authorize]
[ApiController]
public class DataGuidanceController : ControllerBase
{
    private readonly IDataGuidanceService _dataGuidanceService;

    public DataGuidanceController(IDataGuidanceService dataGuidanceService)
    {
        _dataGuidanceService = dataGuidanceService;
    }

    [HttpGet("release/{releaseVersionId:guid}/data-guidance")]
    public async Task<ActionResult<DataGuidanceViewModel>> GetReleaseDataGuidance(Guid releaseVersionId)
    {
        return await _dataGuidanceService.GetDataGuidance(releaseVersionId)
            .HandleFailuresOrOk();
    }

    [HttpPatch("release/{releaseVersionId:guid}/data-guidance")]
    public async Task<ActionResult<DataGuidanceViewModel>> UpdateReleaseDataGuidance(Guid releaseVersionId,
        DataGuidanceUpdateRequest request)
    {
        return await _dataGuidanceService.UpdateDataGuidance(releaseVersionId, request)
            .HandleFailuresOrOk();
    }
}

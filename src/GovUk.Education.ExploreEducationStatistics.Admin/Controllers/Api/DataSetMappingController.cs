#nullable enable
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
public class DataSetMappingController(IDataSetMappingService dataSetMappingService) : ControllerBase
{
    [HttpPatch("releases/{releaseVersionId:guid}/data/replacements/mapping/indicators")]
    public async Task<ActionResult<List<IndicatorMappingDto>>> UpdateIndicatorMappings(
        [FromRoute] Guid releaseVersionId,
        [FromBody] IndicatorMappingUpdatesRequest request,
        CancellationToken cancellationToken = default
    )
    {
        return await dataSetMappingService.UpdateIndicatorMappings(request).HandleFailuresOrOk();
    }
}

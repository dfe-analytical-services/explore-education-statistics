#nullable enable
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using GovUk.Education.ExploreEducationStatistics.Admin.Services.Interfaces.Public.Data;
using GovUk.Education.ExploreEducationStatistics.Admin.Services.Public.Data;
using GovUk.Education.ExploreEducationStatistics.Common.Extensions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace GovUk.Education.ExploreEducationStatistics.Admin.Controllers.Api.Public.Data;

[Authorize]
[ApiController]
[Route("api/public-data/data-set-versions")]
public class DataSetVersionsController(IDataSetVersionService dataSetVersionService) : ControllerBase
{
    [HttpGet("statuses")]
    [Produces("application/json")]
    public async Task<ActionResult<List<DataSetVersionStatusViewModel>>> ListStatusesForReleaseVersion(
        [FromQuery] Guid releaseVersionId)
    {
        return await dataSetVersionService
            .ListStatusesForReleaseVersion(releaseVersionId)
            .HandleFailuresOrOk();
    }
}

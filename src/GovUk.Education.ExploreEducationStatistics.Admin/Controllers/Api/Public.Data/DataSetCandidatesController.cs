#nullable enable
using GovUk.Education.ExploreEducationStatistics.Admin.Services.Interfaces.Public.Data;
using GovUk.Education.ExploreEducationStatistics.Admin.ViewModels.Public.Data;
using GovUk.Education.ExploreEducationStatistics.Common.Extensions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace GovUk.Education.ExploreEducationStatistics.Admin.Controllers.Api.Public.Data;

[Authorize]
[ApiController]
[Route("api/public-data/data-set-candidates")]
public class DataSetCandidatesController(IReleaseService releaseService) : ControllerBase
{
    [HttpGet]
    [Produces("application/json")]
    public async Task<ActionResult<IReadOnlyList<ApiDataSetCandidateViewModel>>> ListApiDataSetCandidates(
        [FromQuery] Guid releaseVersionId,
        CancellationToken cancellationToken)
    {
        return await releaseService
            .ListApiDataSetCandidates(releaseVersionId, cancellationToken)
            .HandleFailuresOrOk();
    }
}

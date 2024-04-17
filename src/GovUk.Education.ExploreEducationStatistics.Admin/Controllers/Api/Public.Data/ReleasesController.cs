#nullable enable
using GovUk.Education.ExploreEducationStatistics.Admin.Services.Interfaces.Public.Data;
using GovUk.Education.ExploreEducationStatistics.Admin.ViewModels;
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
[Route("api/public-data/releases")]
public class ReleasesController(IReleaseService releaseService) : ControllerBase
{
    [HttpGet("{releaseVersionId:guid}/data-set-candidates")]
    [Produces("application/json")]
    public async Task<ActionResult<IReadOnlyList<ApiDataSetCandidateViewModel>>> GetDataSetCandidates(
        Guid releaseVersionId,
        CancellationToken cancellationToken)
    {
        return await releaseService
            .GetApiDataSetCandidates(releaseVersionId, cancellationToken)
            .HandleFailuresOrNoContent();
    }
}

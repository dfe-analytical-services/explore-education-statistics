using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using GovUk.Education.ExploreEducationStatistics.Admin.Services.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Admin.ViewModels;
using GovUk.Education.ExploreEducationStatistics.Common.Extensions;
using GovUk.Education.ExploreEducationStatistics.Common.Model;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace GovUk.Education.ExploreEducationStatistics.Admin.Controllers.Api;

[Route("api")]
[Authorize]
[ApiController]
public class DataReplacementController(IReplacementService replacementService) : ControllerBase
{
    [HttpGet("releases/{releaseVersionId:guid}/data/replacement-plan/{replacementFileId:guid}")] // @MarkFix only need replacementFileId
    public async Task<ActionResult<DataReplacementPlanViewModel>> GetReplacementPlan(Guid releaseVersionId,
        Guid replacementFileId,
        CancellationToken cancellationToken = default)
    {
        return await replacementService.GetReplacementPlan(
                releaseVersionId: releaseVersionId,
                replacementFileId: replacementFileId,
                cancellationToken: cancellationToken
            )
            .OnSuccess(plan => plan.ToSummary())
            .HandleFailuresOrOk();
    }

    [HttpPost("releases/{releaseVersionId:guid}/data/replacements")]
    public async Task<ActionResult<Unit>> Replace(
        Guid releaseVersionId,
        [FromBody] List<Guid> replacementFileIds,
        CancellationToken cancellationToken)
    {
        return await replacementService.Replace(
                releaseVersionId: releaseVersionId,
                replacementFileIds: replacementFileIds,
                cancellationToken: cancellationToken)
            .HandleFailuresOrOk();
    }
}

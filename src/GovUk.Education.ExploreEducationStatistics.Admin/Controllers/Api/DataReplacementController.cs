using System;
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
    [HttpGet("releases/{releaseVersionId:guid}/data/{replacementFileId:guid}/replacement-plan")]
    public async Task<ActionResult<DataReplacementPlanViewModel>> GetReplacementPlan(
        Guid releaseVersionId,
        Guid replacementFileId, // @MarkFix also turn into a list?
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

    [HttpPost("releases/{releaseVersionId:guid}/data/{replacementFileId:guid}/replacement")]
    public async Task<ActionResult<Unit>> Replace(
        Guid releaseVersionId,
        Guid replacementFileId) // @MarkFix turn into a list
    {
        return await replacementService.Replace(
                releaseVersionId: releaseVersionId,
                replacementFileId: replacementFileId
            )
            .HandleFailuresOrOk();
    }
}

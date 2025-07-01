using System;
using System.Threading;
using System.Threading.Tasks;
using GovUk.Education.ExploreEducationStatistics.Admin.Requests;
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
public class DataReplacementController : ControllerBase
{
    private readonly IReplacementService _replacementService;

    public DataReplacementController(IReplacementService replacementService)
    {
        _replacementService = replacementService;
    }

    [HttpGet("releases/{releaseVersionId:guid}/data/{originalFileId:guid}/replacement-plan")]
    public async Task<ActionResult<DataReplacementPlanViewModel>> GetReplacementPlan(Guid releaseVersionId,
        Guid originalFileId,
        CancellationToken cancellationToken = default)
    {
        return await _replacementService.GetReplacementPlan(
                releaseVersionId: releaseVersionId,
                originalFileId: originalFileId,
                cancellationToken: cancellationToken
            )
            .OnSuccess(plan => plan.ToSummary())
            .HandleFailuresOrOk();
    }

    [HttpPost("releases/{releaseVersionId:guid}/data/replacements")]
    public async Task<ActionResult<Unit>> Replace(
        [FromRoute] Guid releaseVersionId,
        [FromBody] ReplacementRequest request,
        CancellationToken cancellationToken = default)
    {
        return await _replacementService.Replace(
                releaseVersionId: releaseVersionId,
                originalFileIds: request.OriginalFileIds,
                cancellationToken: cancellationToken
            )
            .HandleFailuresOrOk();
    }
}

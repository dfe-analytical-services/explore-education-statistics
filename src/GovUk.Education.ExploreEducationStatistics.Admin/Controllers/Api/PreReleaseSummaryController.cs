#nullable enable
using GovUk.Education.ExploreEducationStatistics.Admin.Services.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Admin.ViewModels;
using GovUk.Education.ExploreEducationStatistics.Common.Extensions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace GovUk.Education.ExploreEducationStatistics.Admin.Controllers.Api;

[Route("api")]
[ApiController]
[Authorize]
public class PreReleaseSummaryController(IPreReleaseSummaryService preReleaseSummaryService) : ControllerBase
{
    [HttpGet("release/{releaseVersionId:guid}/pre-release-summary")]
    public async Task<ActionResult<PreReleaseSummaryViewModel>> GetPreReleaseSummary(
        Guid releaseVersionId,
        CancellationToken cancellationToken = default
    )
    {
        return await preReleaseSummaryService
            .GetPreReleaseSummaryViewModel(releaseVersionId, cancellationToken)
            .HandleFailuresOrOk();
    }
}

#nullable enable
using GovUk.Education.ExploreEducationStatistics.Admin.Requests;
using GovUk.Education.ExploreEducationStatistics.Admin.Services.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Admin.ViewModels;
using GovUk.Education.ExploreEducationStatistics.Common.Extensions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace GovUk.Education.ExploreEducationStatistics.Admin.Controllers.Api.Statistics;

[Route("api")]
[ApiController]
[Authorize]
public class PairedIndicatorsController(IPairedIndicatorService pairedIndicatorService) : ControllerBase
{
    [HttpGet("data/release/{releaseVersionId:guid}/meta/subject/{subjectId:guid}/paired-indicators")]
    public async Task<ActionResult<PairedIndicatorsViewModel>> GetPairedIndicators(
        Guid releaseVersionId,
        Guid subjectId,
        CancellationToken cancellationToken
    )
    {
        return await pairedIndicatorService
            .GetPairedIndicators(releaseVersionId: releaseVersionId, subjectId: subjectId, cancellationToken)
            .HandleFailuresOrOk();
    }

    [HttpPut("data/release/{releaseVersionId:guid}/meta/subject/{subjectId:guid}/paired-indicators")]
    public async Task<ActionResult<PairedIndicatorsViewModel>> UpdatePairedIndicators(
        Guid releaseVersionId,
        Guid subjectId,
        PairedIndicatorsUpdateRequest request,
        CancellationToken cancellationToken
    )
    {
        return await pairedIndicatorService
            .UpdatePairedIndicators(
                releaseVersionId: releaseVersionId,
                subjectId: subjectId,
                request,
                cancellationToken
            )
            .HandleFailuresOrOk();
    }
}

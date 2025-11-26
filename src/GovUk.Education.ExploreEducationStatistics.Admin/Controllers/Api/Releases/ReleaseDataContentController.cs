using GovUk.Education.ExploreEducationStatistics.Admin.Services.Releases;
using GovUk.Education.ExploreEducationStatistics.Admin.Services.Releases.Dtos;
using GovUk.Education.ExploreEducationStatistics.Common.Extensions;
using Microsoft.AspNetCore.Mvc;

namespace GovUk.Education.ExploreEducationStatistics.Admin.Controllers.Api.Releases;

[Route("api")]
[ApiController]
public class ReleaseDataContentController(IReleaseDataContentService releaseDataContentService) : ControllerBase
{
    [HttpGet("releaseVersions/{releaseVersionId:guid}/data-content")]
    public async Task<ActionResult<ReleaseDataContentDto>> GetReleaseDataContent(
        Guid releaseVersionId,
        CancellationToken cancellationToken = default
    ) =>
        await releaseDataContentService.GetReleaseDataContent(releaseVersionId, cancellationToken).HandleFailuresOrOk();
}

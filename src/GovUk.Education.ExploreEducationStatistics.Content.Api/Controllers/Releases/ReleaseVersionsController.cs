#nullable enable
using GovUk.Education.ExploreEducationStatistics.Common.Extensions;
using GovUk.Education.ExploreEducationStatistics.Content.Services.Releases;
using GovUk.Education.ExploreEducationStatistics.Content.Services.Releases.Dtos;
using Microsoft.AspNetCore.Mvc;

namespace GovUk.Education.ExploreEducationStatistics.Content.Api.Controllers.Releases;

[Route("api")]
[ApiController]
public class ReleaseVersionsController(IReleaseVersionsService releaseVersionsService) : ControllerBase
{
    [HttpGet("publications/{publicationSlug}/releases/{releaseSlug}/version-summary")]
    public async Task<ActionResult<ReleaseVersionSummaryDto>> GetReleaseVersionSummary(
        string publicationSlug,
        string releaseSlug,
        CancellationToken cancellationToken = default
    ) =>
        await releaseVersionsService
            .GetReleaseVersionSummary(publicationSlug: publicationSlug, releaseSlug: releaseSlug, cancellationToken)
            .HandleFailuresOrOk();
}

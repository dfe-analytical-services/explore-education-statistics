#nullable enable
using GovUk.Education.ExploreEducationStatistics.Common.Extensions;
using GovUk.Education.ExploreEducationStatistics.Content.Services.Releases;
using GovUk.Education.ExploreEducationStatistics.Content.Services.Releases.Dtos;
using Microsoft.AspNetCore.Mvc;

namespace GovUk.Education.ExploreEducationStatistics.Content.Api.Controllers.Releases;

[Route("api")]
[ApiController]
public class ReleaseVersionsController(
    IReleaseContentService releaseContentService,
    IReleaseDataContentService releaseDataContentService,
    IReleaseVersionsService releaseVersionsService
) : ControllerBase
{
    [HttpGet("publications/{publicationSlug}/releases/{releaseSlug}/content")]
    public async Task<ActionResult<ReleaseContentDto>> GetReleaseContent(
        string publicationSlug,
        string releaseSlug,
        CancellationToken cancellationToken = default
    ) =>
        await releaseContentService
            .GetReleaseContent(publicationSlug: publicationSlug, releaseSlug: releaseSlug, cancellationToken)
            .HandleFailuresOrOk();

    [HttpGet("publications/{publicationSlug}/releases/{releaseSlug}/data-content")]
    public async Task<ActionResult<ReleaseDataContentDto>> GetReleaseDataContent(
        string publicationSlug,
        string releaseSlug,
        CancellationToken cancellationToken = default
    ) =>
        await releaseDataContentService
            .GetReleaseDataContent(publicationSlug: publicationSlug, releaseSlug: releaseSlug, cancellationToken)
            .HandleFailuresOrOk();

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

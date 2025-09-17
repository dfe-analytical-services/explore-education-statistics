#nullable enable
using GovUk.Education.ExploreEducationStatistics.Common.Extensions;
using GovUk.Education.ExploreEducationStatistics.Common.ViewModels;
using GovUk.Education.ExploreEducationStatistics.Content.Api.Controllers.Releases.Dtos;
using GovUk.Education.ExploreEducationStatistics.Content.Services.Releases;
using GovUk.Education.ExploreEducationStatistics.Content.Services.Releases.Dtos;
using Microsoft.AspNetCore.Mvc;

namespace GovUk.Education.ExploreEducationStatistics.Content.Api.Controllers.Releases;

[Route("api")]
[ApiController]
public class ReleaseUpdatesController(IReleaseUpdatesService releaseUpdatesService) : ControllerBase
{
    [HttpGet("publications/{publicationSlug}/releases/{releaseSlug}/updates")]
    public async Task<ActionResult<PaginatedListViewModel<ReleaseUpdateDto>>> GetPaginatedUpdatesForRelease(
        [FromQuery] GetReleaseUpdatesRequest request,
        CancellationToken cancellationToken = default) =>
        await releaseUpdatesService.GetPaginatedUpdatesForRelease(
                publicationSlug: request.PublicationSlug,
                releaseSlug: request.ReleaseSlug,
                page: request.Page,
                pageSize: request.PageSize,
                cancellationToken)
            .HandleFailuresOrOk();
}

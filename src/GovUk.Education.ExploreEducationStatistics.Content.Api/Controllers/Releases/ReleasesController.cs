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
public class ReleasesController(IReleasesService releasesService) : ControllerBase
{
    [HttpGet("publications/{publicationSlug}/release-entries")]
    public async Task<ActionResult<PaginatedListViewModel<IReleaseEntryDto>>> GetPaginatedReleaseEntriesForPublication(
        [FromQuery] GetPaginatedReleasesForPublicationRequest request,
        CancellationToken cancellationToken = default) =>
        await releasesService.GetPaginatedReleaseEntriesForPublication(
                publicationSlug: request.PublicationSlug,
                page: request.Page,
                pageSize: request.PageSize,
                cancellationToken)
            .HandleFailuresOrOk();
}

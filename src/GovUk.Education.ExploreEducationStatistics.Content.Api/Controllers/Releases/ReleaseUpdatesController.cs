using GovUk.Education.ExploreEducationStatistics.Common.Extensions;
using GovUk.Education.ExploreEducationStatistics.Common.ViewModels;
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
        [FromRoute] string publicationSlug,
        [FromRoute] string releaseSlug,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 10,
        CancellationToken cancellationToken = default)
    {
        var request = new GetReleaseUpdatesRequest
        {
            PublicationSlug = publicationSlug,
            ReleaseSlug = releaseSlug,
            Page = page,
            PageSize = pageSize
        };

        return await releaseUpdatesService.GetPaginatedUpdatesForRelease(
                request,
                cancellationToken: cancellationToken)
            .HandleFailuresOrOk();
    }
}

using GovUk.Education.ExploreEducationStatistics.Admin.Controllers.Api.Releases.Dtos;
using GovUk.Education.ExploreEducationStatistics.Admin.Services.Releases;
using GovUk.Education.ExploreEducationStatistics.Admin.Services.Releases.Dtos;
using GovUk.Education.ExploreEducationStatistics.Common.Extensions;
using GovUk.Education.ExploreEducationStatistics.Common.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace GovUk.Education.ExploreEducationStatistics.Admin.Controllers.Api.Releases;

[Route("api")]
[ApiController]
[Authorize]
public class ReleaseUpdatesController(IReleaseUpdatesService releaseUpdatesService) : ControllerBase
{
    [HttpPost("releases/{releaseVersionId:guid}/updates")]
    public async Task<ActionResult<List<ReleaseUpdateDto>>> CreateReleaseUpdate(
        ReleaseUpdatesCreateRequest request,
        CancellationToken cancellationToken = default
    ) =>
        await releaseUpdatesService
            .CreateReleaseUpdate(
                releaseVersionId: request.ReleaseVersionId,
                date: request.On,
                reason: request.Reason,
                cancellationToken: cancellationToken
            )
            .HandleFailuresOr(result => Created(HttpContext.Request.Path, result));

    [HttpGet("releases/{releaseVersionId:guid}/updates")]
    public async Task<ActionResult<PaginatedListViewModel<ReleaseUpdateDto>>> GetReleaseUpdates(
        [FromQuery] ReleaseUpdatesGetRequest request,
        CancellationToken cancellationToken = default
    ) =>
        await releaseUpdatesService
            .GetReleaseUpdates(
                releaseVersionId: request.ReleaseVersionId,
                page: request.Page,
                pageSize: request.PageSize,
                cancellationToken: cancellationToken
            )
            .HandleFailuresOrOk();

    [HttpPut("releases/{releaseVersionId:guid}/updates/{releaseUpdateId:guid}")]
    public async Task<ActionResult<List<ReleaseUpdateDto>>> UpdateReleaseUpdate(
        ReleaseUpdatesUpdateRequest request,
        CancellationToken cancellationToken = default
    ) =>
        await releaseUpdatesService
            .UpdateReleaseUpdate(
                releaseVersionId: request.ReleaseVersionId,
                releaseUpdateId: request.ReleaseUpdateId,
                date: request.On,
                reason: request.Reason,
                cancellationToken: cancellationToken
            )
            .HandleFailuresOrOk();

    [HttpDelete("releases/{releaseVersionId:guid}/updates/{releaseUpdateId:guid}")]
    public async Task<ActionResult<List<ReleaseUpdateDto>>> DeleteReleaseUpdate(
        Guid releaseVersionId,
        Guid releaseUpdateId,
        CancellationToken cancellationToken = default
    ) =>
        await releaseUpdatesService
            .DeleteReleaseUpdate(
                releaseVersionId: releaseVersionId,
                releaseUpdateId: releaseUpdateId,
                cancellationToken: cancellationToken
            )
            .HandleFailuresOrOk();
}

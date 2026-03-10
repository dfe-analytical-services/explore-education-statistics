#nullable enable
using GovUk.Education.ExploreEducationStatistics.Admin.Requests;
using GovUk.Education.ExploreEducationStatistics.Admin.Services.Interfaces.ManageContent;
using GovUk.Education.ExploreEducationStatistics.Admin.ViewModels.ManageContent;
using GovUk.Education.ExploreEducationStatistics.Common.Extensions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace GovUk.Education.ExploreEducationStatistics.Admin.Controllers.Api.ManageContent;

[Route("api/release/{releaseVersionId:guid}/content/release-note")]
[ApiController]
[Authorize]
public class ReleaseNoteController(IReleaseNoteService releaseNoteService) : ControllerBase
{
    [HttpPost]
    public async Task<ActionResult<List<ReleaseNoteViewModel>>> CreateReleaseNote(
        ReleaseNoteCreateRequest createRequest,
        Guid releaseVersionId,
        CancellationToken cancellationToken = default
    ) =>
        await releaseNoteService
            .CreateReleaseNote(releaseVersionId: releaseVersionId, createRequest, cancellationToken)
            .HandleFailuresOr(result => Created(HttpContext.Request.Path, result));

    [HttpPut("{releaseNoteId:guid}")]
    public async Task<ActionResult<List<ReleaseNoteViewModel>>> UpdateReleaseNote(
        ReleaseNoteUpdateRequest updateRequest,
        Guid releaseVersionId,
        Guid releaseNoteId,
        CancellationToken cancellationToken = default
    ) =>
        await releaseNoteService
            .UpdateReleaseNote(
                releaseVersionId: releaseVersionId,
                releaseNoteId: releaseNoteId,
                updateRequest,
                cancellationToken
            )
            .HandleFailuresOrOk();

    [HttpDelete("{releaseNoteId:guid}")]
    public async Task<ActionResult<List<ReleaseNoteViewModel>>> DeleteReleaseNote(
        Guid releaseVersionId,
        Guid releaseNoteId,
        CancellationToken cancellationToken = default
    ) =>
        await releaseNoteService
            .DeleteReleaseNote(releaseVersionId: releaseVersionId, releaseNoteId: releaseNoteId, cancellationToken)
            .HandleFailuresOrOk();
}

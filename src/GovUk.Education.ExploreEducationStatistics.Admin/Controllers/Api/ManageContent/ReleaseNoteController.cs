using GovUk.Education.ExploreEducationStatistics.Admin.Services.Interfaces.ManageContent;
using GovUk.Education.ExploreEducationStatistics.Admin.ViewModels.ManageContent;
using GovUk.Education.ExploreEducationStatistics.Common.Extensions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace GovUk.Education.ExploreEducationStatistics.Admin.Controllers.Api.ManageContent;

[Route("api")]
[ApiController]
[Authorize]
public class ReleaseNoteController(IReleaseNoteService releaseNoteService) : ControllerBase
{
    [HttpPost("release/{releaseVersionId:guid}/content/release-note")]
    public async Task<ActionResult<List<ReleaseNoteViewModel>>> AddReleaseNote(
        ReleaseNoteSaveRequest saveRequest,
        Guid releaseVersionId
    ) =>
        await releaseNoteService
            .AddReleaseNote(releaseVersionId, saveRequest)
            .HandleFailuresOr(result => Created(HttpContext.Request.Path, result));

    [HttpPut("release/{releaseVersionId:guid}/content/release-note/{releaseNoteId:guid}")]
    public async Task<ActionResult<List<ReleaseNoteViewModel>>> UpdateReleaseNote(
        ReleaseNoteSaveRequest saveRequest,
        Guid releaseVersionId,
        Guid releaseNoteId
    ) =>
        await releaseNoteService
            .UpdateReleaseNote(releaseVersionId: releaseVersionId, releaseNoteId: releaseNoteId, saveRequest)
            .HandleFailuresOrOk();

    [HttpDelete("release/{releaseVersionId:guid}/content/release-note/{releaseNoteId:guid}")]
    public async Task<ActionResult<List<ReleaseNoteViewModel>>> DeleteReleaseNote(
        Guid releaseVersionId,
        Guid releaseNoteId
    ) =>
        await releaseNoteService
            .DeleteReleaseNote(releaseVersionId: releaseVersionId, releaseNoteId: releaseNoteId)
            .HandleFailuresOrOk();
}

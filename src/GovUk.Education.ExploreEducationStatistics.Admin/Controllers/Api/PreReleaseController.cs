#nullable enable
using GovUk.Education.ExploreEducationStatistics.Admin.Services.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Admin.ViewModels;
using GovUk.Education.ExploreEducationStatistics.Common.Extensions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace GovUk.Education.ExploreEducationStatistics.Admin.Controllers.Api;

[Route("api")]
[ApiController]
[Authorize]
public class PreReleaseController : ControllerBase
{
    private readonly IPreReleaseUserService _preReleaseUserService;
    private readonly IPreReleaseSummaryService _preReleaseSummaryService;

    public PreReleaseController(IPreReleaseUserService preReleaseUserService,
        IPreReleaseSummaryService preReleaseSummaryService)
    {
        _preReleaseUserService = preReleaseUserService;
        _preReleaseSummaryService = preReleaseSummaryService;
    }

    [HttpGet("release/{releaseVersionId:guid}/prerelease-users")]
    public async Task<ActionResult<List<PreReleaseUserViewModel>>> GetPreReleaseUsers(Guid releaseVersionId)
    {
        return await _preReleaseUserService
            .GetPreReleaseUsers(releaseVersionId)
            .HandleFailuresOrOk();
    }

    [HttpGet("release/{releaseVersionId:guid}/prerelease")]
    public async Task<ActionResult<PreReleaseSummaryViewModel>> GetPreReleaseSummary(
        Guid releaseVersionId,
        CancellationToken cancellationToken = default)
    {
        return await _preReleaseSummaryService
            .GetPreReleaseSummaryViewModel(releaseVersionId, cancellationToken)
            .HandleFailuresOrOk();
    }

    [HttpPost("release/{releaseVersionId:guid}/prerelease-users-plan")]
    public async Task<ActionResult<PreReleaseUserInvitePlan>> GetPreReleaseUsersInvitePlan(
        Guid releaseVersionId, [FromBody] PreReleaseUserInviteViewModel viewModel)
    {
        return await _preReleaseUserService
            .GetPreReleaseUsersInvitePlan(releaseVersionId, viewModel.Emails)
            .HandleFailuresOrOk();
    }

    [HttpPost("release/{releaseVersionId:guid}/prerelease-users")]
    public async Task<ActionResult<List<PreReleaseUserViewModel>>> InvitePreReleaseUsers(
        Guid releaseVersionId, [FromBody] PreReleaseUserInviteViewModel viewModel)
    {
        return await _preReleaseUserService
            .InvitePreReleaseUsers(releaseVersionId, viewModel.Emails)
            .HandleFailuresOrOk();
    }

    [HttpDelete("release/{releaseVersionId:guid}/prerelease-users")]
    public async Task<ActionResult> RemovePreReleaseUser(
        Guid releaseVersionId, [FromBody] PreReleaseUserRemoveRequest request)
    {
        return await _preReleaseUserService
            .RemovePreReleaseUser(releaseVersionId, request.Email)
            .HandleFailuresOrNoContent();
    }
}

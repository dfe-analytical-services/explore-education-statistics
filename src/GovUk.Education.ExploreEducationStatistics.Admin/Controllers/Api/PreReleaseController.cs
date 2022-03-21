#nullable enable
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using GovUk.Education.ExploreEducationStatistics.Admin.Services.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Admin.ViewModels;
using GovUk.Education.ExploreEducationStatistics.Common.Extensions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace GovUk.Education.ExploreEducationStatistics.Admin.Controllers.Api
{
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

        [HttpGet("release/{releaseId:guid}/prerelease-users")]
        public async Task<ActionResult<List<PreReleaseUserViewModel>>> GetPreReleaseUsers(Guid releaseId)
        {
            return await _preReleaseUserService
                .GetPreReleaseUsers(releaseId)
                .HandleFailuresOrOk();
        }

        [HttpGet("release/{releaseId:guid}/prerelease")]
        public async Task<ActionResult<PreReleaseSummaryViewModel>> GetPreReleaseSummaryAsync(Guid releaseId)
        {
            return await _preReleaseSummaryService
                .GetPreReleaseSummaryViewModelAsync(releaseId)
                .HandleFailuresOrOk();
        }

        [HttpPost("release/{releaseId:guid}/prerelease-users-plan")]
        public async Task<ActionResult<PreReleaseUserInvitePlan>> GetPreReleaseUsersInvitePlan(
            Guid releaseId, [FromBody] PreReleaseUserInviteViewModel viewModel)
        {
            return await _preReleaseUserService
                .GetPreReleaseUsersInvitePlan(releaseId, viewModel.Emails)
                .HandleFailuresOrOk();
        }

        [HttpPost("release/{releaseId:guid}/prerelease-users")]
        public async Task<ActionResult<List<PreReleaseUserViewModel>>> InvitePreReleaseUsers(
            Guid releaseId, [FromBody] PreReleaseUserInviteViewModel viewModel)
        {
            return await _preReleaseUserService
                .InvitePreReleaseUsers(releaseId, viewModel.Emails)
                .HandleFailuresOrOk();
        }

        [HttpDelete("release/{releaseId:guid}/prerelease-users")]
        public async Task<ActionResult> RemovePreReleaseUser(
            Guid releaseId, [FromBody] PreReleaseUserRemoveRequest request)
        {
            return await _preReleaseUserService
                .RemovePreReleaseUser(releaseId, request.Email)
                .HandleFailuresOrNoContent();
        }
    }
}

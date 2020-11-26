using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using GovUk.Education.ExploreEducationStatistics.Admin.Services;
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

        [HttpGet("release/{releaseId}/prerelease-users")]
        public async Task<ActionResult<List<PreReleaseUserViewModel>>> GetPreReleaseUsers(Guid releaseId)
        {
            return await _preReleaseUserService
                .GetPreReleaseUsers(releaseId)
                .HandleFailuresOrOk();
        }

        [HttpGet("release/{releaseId}/prerelease")]
        public async Task<ActionResult<PreReleaseSummaryViewModel>> GetPreReleaseSummaryAsync(Guid releaseId)
        {
            return await _preReleaseSummaryService
                .GetPreReleaseSummaryViewModelAsync(releaseId)
                .HandleFailuresOrOk();
        }

        [HttpPost("release/{releaseId}/prerelease-users")]
        public async Task<ActionResult<PreReleaseUserViewModel>> InvitePreReleaseUser(
            Guid releaseId, [FromBody] PreReleaseAccessRequest request)
        {
            return await _preReleaseUserService
                .AddPreReleaseUser(releaseId, request.Email)
                .HandleFailuresOrOk();
        }

        [HttpDelete("release/{releaseId}/prerelease-users")]
        public async Task<ActionResult> RemovePreReleaseUser(
            Guid releaseId, [FromQuery] PreReleaseAccessRequest request)
        {
            return await _preReleaseUserService
                .RemovePreReleaseUser(releaseId, request.Email)
                .HandleFailuresOrNoContent();
        }
    }

    public class PreReleaseAccessRequest
    {
        public string Email { get; set; }
    }
}
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
        private readonly IPreReleaseContactsService _preReleaseContactsService;
        private readonly IPreReleaseSummaryService _preReleaseSummaryService;

        public PreReleaseController(IPreReleaseContactsService preReleaseContactsService,
            IPreReleaseSummaryService preReleaseSummaryService)
        {
            _preReleaseContactsService = preReleaseContactsService;
            _preReleaseSummaryService = preReleaseSummaryService;
        }

        [HttpGet("release/{releaseId}/prerelease-contacts")]
        public async Task<ActionResult<List<PrereleaseCandidateViewModel>>> GetPreReleaseContactsForRelease(Guid releaseId)
        {
            return await _preReleaseContactsService
                .GetPreReleaseContactsForReleaseAsync(releaseId)
                .HandleFailuresOrOk();
        }

        [HttpGet("release/{releaseId}/prerelease")]
        public async Task<ActionResult<PreReleaseSummaryViewModel>> GetPreReleaseSummaryAsync(Guid releaseId)
        {
            return await _preReleaseSummaryService
                .GetPreReleaseSummaryViewModelAsync(releaseId)
                .HandleFailuresOrOk();
        }

        [HttpPost("release/{releaseId}/prerelease-contact")]
        public async Task<ActionResult<List<PrereleaseCandidateViewModel>>> AddPreReleaseContactToRelease(
            Guid releaseId, [FromBody] PrereleaseAccessRequest request)
        {
            return await _preReleaseContactsService
                .AddPreReleaseContactToReleaseAsync(releaseId, request.Email)
                .HandleFailuresOrOk();
        }

        [HttpDelete("release/{releaseId}/prerelease-contact")]
        public async Task<ActionResult<List<PrereleaseCandidateViewModel>>> RemovePreReleaseContactFromRelease(
            Guid releaseId, [FromBody] PrereleaseAccessRequest request)
        {
            return await _preReleaseContactsService
                .RemovePreReleaseContactFromReleaseAsync(releaseId, request.Email)
                .HandleFailuresOrOk();
        }
    }

    public class PrereleaseAccessRequest
    {
        public string Email { get; set; }
    }
}
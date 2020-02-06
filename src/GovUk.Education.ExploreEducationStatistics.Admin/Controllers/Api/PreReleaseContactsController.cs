using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using GovUk.Education.ExploreEducationStatistics.Admin.Controllers.Utils;
using GovUk.Education.ExploreEducationStatistics.Admin.Services;
using GovUk.Education.ExploreEducationStatistics.Admin.Services.Interfaces;
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

        public PreReleaseController(IPreReleaseContactsService preReleaseContactsService)
        {
            _preReleaseContactsService = preReleaseContactsService;
        }

        [HttpGet("prerelease/contacts")]
        public async Task<ActionResult<List<PrereleaseCandidateViewModel>>> GetAvailablePreReleaseContacts()
        {
            return await _preReleaseContactsService
                .GetAvailablePreReleaseContactsAsync()
                .HandleFailuresOr(Ok);
        }

        [HttpGet("release/{releaseId}/prerelease-contacts")]
        public async Task<ActionResult<List<PrereleaseCandidateViewModel>>> GetPreReleaseContactsForRelease(Guid releaseId)
        {
            return await _preReleaseContactsService
                .GetPreReleaseContactsForReleaseAsync(releaseId)
                .HandleFailuresOr(Ok);
        }

        [HttpPost("release/{releaseId}/prerelease-contact")]
        public async Task<ActionResult<List<PrereleaseCandidateViewModel>>> AddPreReleaseContactToRelease(
            Guid releaseId, [FromBody] PrereleaseAccessRequest request)
        {
            return await _preReleaseContactsService
                .AddPreReleaseContactToReleaseAsync(releaseId, request.Email)
                .HandleFailuresOr(Ok);
        }

        [HttpDelete("release/{releaseId}/prerelease-contact")]
        public async Task<ActionResult<List<PrereleaseCandidateViewModel>>> RemovePreReleaseContactFromRelease(
            Guid releaseId, [FromBody] PrereleaseAccessRequest request)
        {
            return await _preReleaseContactsService
                .RemovePreReleaseContactFromReleaseAsync(releaseId, request.Email)
                .HandleFailuresOr(Ok);
        }
    }

    public class PrereleaseAccessRequest
    {
        public string Email { get; set; }
    }
}
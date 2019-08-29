using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using GovUk.Education.ExploreEducationStatistics.Admin.Models.Api;
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
        private readonly IPreReleaseService _preReleaseService;

        public PreReleaseController(IPreReleaseService preReleaseService)
        {
            _preReleaseService = preReleaseService;
        }

        // TODO Authorisation will be required when users are introduced
        [HttpGet("prerelease/contacts")]
        public async Task<ActionResult<List<UserDetailsViewModel>>> GetAvailablePreReleaseContacts()
        {
            return await _preReleaseService.GetAvailablePreReleaseContactsAsync();
        }

        // TODO Authorisation will be required when users are introduced
        [HttpGet("release/{releaseId}/prerelease-contacts")]
        public async Task<ActionResult<List<UserDetailsViewModel>>> GetPreReleaseContactsForRelease(Guid releaseId)
        {
            return await _preReleaseService.GetPreReleaseContactsForReleaseAsync(releaseId);
        }

        // TODO Authorisation will be required when users are introduced
        [HttpPost("release/{releaseId}/prerelease-contact/{contactId}")]
        public async Task<ActionResult<List<UserDetailsViewModel>>> AddPreReleaseContactToRelease(Guid releaseId, Guid contactId)
        {
            return await _preReleaseService.AddPreReleaseContactToReleaseAsync(releaseId, contactId);
        }

        // TODO Authorisation will be required when users are introduced
        [HttpDelete("release/{releaseId}/prerelease-contact/{contactId}")]
        public async Task<ActionResult<List<UserDetailsViewModel>>> RemovePreReleaseContactFromRelease(Guid releaseId, Guid contactId)
        {
            return await _preReleaseService.RemovePreReleaseContactFromReleaseAsync(releaseId, contactId);
        }
    }
}
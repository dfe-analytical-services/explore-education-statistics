using System.Collections.Generic;
using System.Threading.Tasks;
using GovUk.Education.ExploreEducationStatistics.Admin.Models.Api;
using GovUk.Education.ExploreEducationStatistics.Admin.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ReleaseId = System.Guid;
using PublicationId = System.Guid;

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

        [HttpGet("prerelease/contacts")]
        public async Task<ActionResult<List<UserDetailsViewModel>>> GetPreReleaseContacts()
        {
            return await _preReleaseService.GetPreReleaseContactsAsync();
        }
    }
}
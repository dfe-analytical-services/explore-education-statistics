using System.Collections.Generic;
using GovUk.Education.ExploreEducationStatistics.Admin.Models.Api;
using GovUk.Education.ExploreEducationStatistics.Admin.Services.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Content.Model;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace GovUk.Education.ExploreEducationStatistics.Admin.Controllers.Api
{
    
    // TODO rename to Releases once the current Crud releases controller is removed
    [ApiController]
    [Authorize]
    public class ReleasesController : ControllerBase
    {
        private readonly IReleaseService _releaseService;

        public ReleasesController(IReleaseService releaseService)
        {
            _releaseService = releaseService;
        }

        // POST api/contacts
        [HttpPost("publication/{publicationId}/releases")]
        [AllowAnonymous] // TODO revisit when authentication and authorisation is in place
        public ActionResult<ReleaseViewModel> CreateRelease(CreateReleaseViewModel release)
        {
            return _releaseService.CreateRelease(release);
        }
    }
}
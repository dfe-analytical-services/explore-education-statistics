using System;
using System.Threading.Tasks;
using GovUk.Education.ExploreEducationStatistics.Admin.Models.Api;
using GovUk.Education.ExploreEducationStatistics.Admin.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ReleaseId = System.Guid;
using PublicationId = System.Guid;

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

        // POST api/publication/{publicationId}/releases
        [HttpPost("publication/{publicationId}/releases")]
        [AllowAnonymous] // TODO revisit when authentication and authorisation is in place
        public async Task<ActionResult<ReleaseViewModel>> CreateReleaseAsync(CreateReleaseViewModel release, PublicationId publicationId)
        {
            release.PublicationId = publicationId;
            return await _releaseService.CreateReleaseAsync(release);
        }

        [HttpGet("releases/{releaseId}/summary")]
        [AllowAnonymous] // TODO revisit when authentication and authorisation is in place
        public async Task<ActionResult<EditReleaseViewModel>> GetReleaseSummaryAsync(ReleaseId releaseId)
        {
            return await _releaseService.GetReleaseSummaryAsync(releaseId);
        }
        
        
        [HttpPut("releases/{releaseId}/summary")]
        [AllowAnonymous] // TODO revisit when authentication and authorisation is in place
        public async Task<ActionResult<ReleaseViewModel>> EditReleaseSummaryAsync(EditReleaseViewModel model, ReleaseId releaseId)
        {
            model.Id = releaseId;
            return await _releaseService.EditReleaseSummaryAsync(model);
        }
    }
}
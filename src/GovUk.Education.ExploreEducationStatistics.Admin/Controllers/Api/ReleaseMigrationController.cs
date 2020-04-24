using System;
using System.Threading.Tasks;
using GovUk.Education.ExploreEducationStatistics.Admin.Services.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Common.Extensions;
using GovUk.Education.ExploreEducationStatistics.Common.Model;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace GovUk.Education.ExploreEducationStatistics.Admin.Controllers.Api
{
    // TODO BAU-405 - temporary code to help seed the Release-File tables from Blob storage 
    [Route("api")]
    [ApiController]
    [Authorize]
    public class ReleaseMigrationController : ControllerBase
    {   
        private readonly IReleaseMigrationService _releaseMigrationService;

        public ReleaseMigrationController(IReleaseMigrationService releaseMigrationService)
        {
            _releaseMigrationService = releaseMigrationService;
        }
        
        [HttpPost("releases/populate-release-amendment-tables")]
        public async Task<ActionResult<bool>> PopulateReleaseAmendmentTables()
        {
            return await _releaseMigrationService
                .PopulateReleaseAmendmentTables()
                .HandleFailuresOrOk();
        }

        [HttpPost("release/{releaseId}/populate-release-amendment-tables")]
        public async Task<ActionResult<bool>> PopulateReleaseAmendmentTables(Guid releaseId)
        {
            return await _releaseMigrationService
                .PopulateReleaseAmendmentTables(releaseId)
                .OnSuccess(_ => true)
                .HandleFailuresOrOk();
        }
    }
}
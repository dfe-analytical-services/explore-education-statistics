using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Threading.Tasks;
using GovUk.Education.ExploreEducationStatistics.Admin.Models;
using GovUk.Education.ExploreEducationStatistics.Admin.Models.Api;
using GovUk.Education.ExploreEducationStatistics.Admin.Services;
using GovUk.Education.ExploreEducationStatistics.Admin.Services.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Admin.ViewModels;
using GovUk.Education.ExploreEducationStatistics.Common.Extensions;
using GovUk.Education.ExploreEducationStatistics.Common.Model;
using GovUk.Education.ExploreEducationStatistics.Common.Services.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Content.Model;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using FileInfo = GovUk.Education.ExploreEducationStatistics.Admin.Models.FileInfo;

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

        [HttpPost("release/{releaseId}/populate-release-file-tables")]
        public async Task<ActionResult<List<ReleaseFile>>> PopulateReleaseFileTables(Guid releaseId)
        {
            return await _releaseMigrationService
                .PopulateReleaseFileTables(releaseId)
                .HandleFailuresOrOk();
        }
    }
}
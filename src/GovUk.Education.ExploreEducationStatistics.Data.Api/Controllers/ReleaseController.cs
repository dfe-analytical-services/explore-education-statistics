#nullable enable
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using GovUk.Education.ExploreEducationStatistics.Common.Extensions;
using GovUk.Education.ExploreEducationStatistics.Data.Services.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Data.Services.ViewModels;
using Microsoft.AspNetCore.Mvc;

namespace GovUk.Education.ExploreEducationStatistics.Data.Api.Controllers
{
    [Route("api")]
    [ApiController]
    public class ReleaseController : ControllerBase
    {
        private readonly IReleaseService _releaseService;

        public ReleaseController(IReleaseService releaseService)
        {
            _releaseService = releaseService;
        }

        [HttpGet("releases/{releaseId}/subjects")]
        public async Task<ActionResult<List<SubjectViewModel>>> ListSubjects(Guid releaseId)
        {
            return await _releaseService
                .ListSubjects(releaseId)
                .HandleFailuresOrOk();
        }

        [HttpGet("releases/{releaseId}/featured-tables")]
        public async Task<ActionResult<List<FeaturedTableViewModel>>> ListFeaturedTables(Guid releaseId)
        {
            return await _releaseService
                .ListFeaturedTables(releaseId)
                .HandleFailuresOrOk();
        }
    }
}
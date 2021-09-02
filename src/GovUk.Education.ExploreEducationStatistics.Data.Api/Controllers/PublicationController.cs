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
    public class PublicationController : ControllerBase
    {
        private readonly IPublicationService _publicationService;

        public PublicationController(IPublicationService publicationService)
        {
            _publicationService = publicationService;
        }

        [HttpGet("publications/{publicationId}/subjects")]
        public async Task<ActionResult<List<SubjectViewModel>>> ListLatestReleaseSubjects(Guid publicationId)
        {
            return await _publicationService
                .ListLatestReleaseSubjects(publicationId)
                .HandleFailuresOrOk();
        }

        [HttpGet("publications/{publicationId}/featured-tables")]
        public async Task<ActionResult<List<FeaturedTableViewModel>>> ListLatestReleaseFeaturedTables(Guid publicationId)
        {
            return await _publicationService
                .ListLatestReleaseFeaturedTables(publicationId)
                .HandleFailuresOrOk();
        }
    }
}
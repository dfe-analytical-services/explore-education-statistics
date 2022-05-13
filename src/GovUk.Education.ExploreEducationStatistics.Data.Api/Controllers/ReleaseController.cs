#nullable enable
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using GovUk.Education.ExploreEducationStatistics.Common.Cache;
using GovUk.Education.ExploreEducationStatistics.Common.Extensions;
using GovUk.Education.ExploreEducationStatistics.Common.Model;
using GovUk.Education.ExploreEducationStatistics.Data.Api.Services.Cache;
using GovUk.Education.ExploreEducationStatistics.Data.Api.Services.Interfaces;
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
        private readonly ICacheKeyService _cacheKeyService;

        public ReleaseController(
            IReleaseService releaseService, 
            ICacheKeyService cacheKeyService)
        {
            _releaseService = releaseService;
            _cacheKeyService = cacheKeyService;
        }

        [HttpGet("releases/{releaseId}/subjects")]
        public async Task<ActionResult<List<SubjectViewModel>>> ListSubjects(Guid releaseId)
        {
            return await _cacheKeyService
                .CreateCacheKeyForReleaseSubjects(releaseId)
                .OnSuccess(ListSubjects)
                .HandleFailuresOrOk();
        }

        [HttpGet("releases/{releaseId}/featured-tables")]
        public async Task<ActionResult<List<FeaturedTableViewModel>>> ListFeaturedTables(Guid releaseId)
        {
            return await _releaseService
                .ListFeaturedTables(releaseId)
                .HandleFailuresOrOk();
        }

        [BlobCache(typeof(ReleaseSubjectsCacheKey))]
        private Task<Either<ActionResult, List<SubjectViewModel>>> ListSubjects(ReleaseSubjectsCacheKey cacheKey)
        {
            return _releaseService.ListSubjects(cacheKey.ReleaseId);
        }
    }
}

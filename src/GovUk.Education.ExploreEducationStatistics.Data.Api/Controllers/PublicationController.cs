#nullable enable
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using GovUk.Education.ExploreEducationStatistics.Common.Cache;
using GovUk.Education.ExploreEducationStatistics.Common.Extensions;
using GovUk.Education.ExploreEducationStatistics.Common.Model;
using GovUk.Education.ExploreEducationStatistics.Content.Model.Repository.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Data.Api.Services.Cache;
using GovUk.Education.ExploreEducationStatistics.Data.Api.Services.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Data.Services.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Data.Services.ViewModels;
using Microsoft.AspNetCore.Mvc;

namespace GovUk.Education.ExploreEducationStatistics.Data.Api.Controllers
{
    [Route("api")]
    [ApiController]
    public class PublicationController : ControllerBase
    {
        private readonly IReleaseRepository _releaseRepository;
        private readonly IReleaseService _releaseService;
        private readonly ICacheKeyService _cacheKeyService;

        public PublicationController(
            IReleaseRepository releaseRepository,
            IReleaseService releaseService,
            ICacheKeyService cacheKeyService)
        {
            _releaseRepository = releaseRepository;
            _releaseService = releaseService;
            _cacheKeyService = cacheKeyService;
        }

        [HttpGet("publications/{publicationId}/subjects")]
        public async Task<ActionResult<List<SubjectViewModel>>> ListLatestReleaseSubjects(Guid publicationId)
        {
            return await _releaseRepository
                .GetLatestPublishedRelease(publicationId)
                .OnSuccess(release => _cacheKeyService.CreateCacheKeyForReleaseSubjects(release.Id))
                .OnSuccess(ListLatestReleaseSubjects)
                .HandleFailuresOrOk();
        }

        [HttpGet("publications/{publicationId}/featured-tables")]
        public async Task<ActionResult<List<FeaturedTableViewModel>>> ListLatestReleaseFeaturedTables(
            Guid publicationId)
        {
            return await _releaseRepository
                .GetLatestPublishedRelease(publicationId)
                .OnSuccess(release => _releaseService.ListFeaturedTables(release.Id))
                .HandleFailuresOrOk();
        }

        [BlobCache(typeof(ReleaseSubjectsCacheKey))]
        private async Task<Either<ActionResult, List<SubjectViewModel>>> ListLatestReleaseSubjects(
            ReleaseSubjectsCacheKey cacheKey)
        {
            return await _releaseService.ListSubjects(cacheKey.ReleaseId);
        }
    }
}

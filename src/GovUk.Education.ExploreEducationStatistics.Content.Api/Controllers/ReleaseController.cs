#nullable enable
using System;
using System.Collections.Generic;
using System.Net.Mime;
using System.Threading.Tasks;
using GovUk.Education.ExploreEducationStatistics.Common.Cache;
using GovUk.Education.ExploreEducationStatistics.Common.Extensions;
using GovUk.Education.ExploreEducationStatistics.Common.Model;
using GovUk.Education.ExploreEducationStatistics.Content.Api.Cache;
using GovUk.Education.ExploreEducationStatistics.Content.Services.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Content.Services.Interfaces.Cache;
using GovUk.Education.ExploreEducationStatistics.Content.Services.ViewModels;
using Microsoft.AspNetCore.Mvc;

namespace GovUk.Education.ExploreEducationStatistics.Content.Api.Controllers
{
    [Route("api")]
    [Produces(MediaTypeNames.Application.Json)]
    public class ReleaseController : ControllerBase
    {
        private const string HalfHourlyExpirySchedule = "*/30 * * * *";

        private readonly IMethodologyCacheService _methodologyCacheService;
        private readonly IPublicationCacheService _publicationCacheService;
        private readonly IReleaseCacheService _releaseCacheService;
        private readonly IReleaseService _releaseService;

        public ReleaseController(IMethodologyCacheService methodologyCacheService,
            IPublicationCacheService publicationCacheService,
            IReleaseCacheService releaseCacheService,
            IReleaseService releaseService)
        {
            _methodologyCacheService = methodologyCacheService;
            _publicationCacheService = publicationCacheService;
            _releaseCacheService = releaseCacheService;
            _releaseService = releaseService;
        }

        [HttpGet("publications/{publicationSlug}/releases")]
        public async Task<ActionResult<List<ReleaseSummaryViewModel>>> ListReleases(string publicationSlug)
        {
            return await _releaseService.List(publicationSlug)
                .HandleFailuresOrOk();
        }

        [MemoryCache(typeof(GetLatestReleaseCacheKey), durationInSeconds: 10,
            expiryScheduleCron: HalfHourlyExpirySchedule)]
        [HttpGet("publications/{publicationSlug}/releases/latest")]
        public async Task<ActionResult<ReleaseViewModel>> GetLatestRelease(string publicationSlug)
        {
            return await GetReleaseViewModel(publicationSlug)
                .HandleFailuresOrOk();
        }

        [HttpGet("publications/{publicationSlug}/releases/latest/summary")]
        public async Task<ActionResult<ReleaseSummaryViewModel>> GetLatestReleaseSummary(string publicationSlug)
        {
            return await GetReleaseSummaryViewModel(publicationSlug)
                .HandleFailuresOrOk();
        }

        [MemoryCache(typeof(GetReleaseCacheKey), durationInSeconds: 15, expiryScheduleCron: HalfHourlyExpirySchedule)]
        [HttpGet("publications/{publicationSlug}/releases/{releaseSlug}")]
        public async Task<ActionResult<ReleaseViewModel>> GetRelease(string publicationSlug, string releaseSlug)
        {
            return await GetReleaseViewModel(publicationSlug, releaseSlug)
                .HandleFailuresOrOk();
        }

        [HttpGet("publications/{publicationSlug}/releases/{releaseSlug}/summary")]
        public async Task<ActionResult<ReleaseSummaryViewModel>> GetReleaseSummary(string publicationSlug,
            string releaseSlug)
        {
            return await GetReleaseSummaryViewModel(
                    publicationSlug,
                    releaseSlug)
                .HandleFailuresOrOk();
        }

        private Task<Either<ActionResult, ReleaseViewModel>> GetReleaseViewModel(
            string publicationSlug,
            string? releaseSlug = null)
        {
            return _publicationCacheService.GetPublication(publicationSlug)
                .OnSuccessCombineWith(publication => _methodologyCacheService.GetSummariesByPublication(publication.Id))
                .OnSuccessCombineWith(_ => _releaseCacheService.GetRelease(publicationSlug, releaseSlug))
                .OnSuccess(tuple =>
                {
                    var (publication, methodologySummaries, release) = tuple;
                    return new ReleaseViewModel(
                        release,
                        new PublicationViewModel(publication, methodologySummaries)
                    );
                });
        }

        private Task<Either<ActionResult, ReleaseSummaryViewModel>> GetReleaseSummaryViewModel(
            string publicationSlug,
            string? releaseSlug = null)
        {
            return _publicationCacheService.GetPublication(publicationSlug)
                .OnSuccessCombineWith(_ => _releaseCacheService.GetRelease(publicationSlug, releaseSlug))
                .OnSuccess(publicationAndRelease =>
                {
                    var (publication, release) = publicationAndRelease;
                    return new ReleaseSummaryViewModel(
                        release,
                        publication
                    );
                });
        }
    }
}

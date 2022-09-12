#nullable enable
using System.Collections.Generic;
using System.Net.Mime;
using System.Threading.Tasks;
using GovUk.Education.ExploreEducationStatistics.Common.Cache;
using GovUk.Education.ExploreEducationStatistics.Common.Extensions;
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

        private readonly IReleaseCacheService _releaseCacheService;
        private readonly IReleaseService _releaseService;

        public ReleaseController(IReleaseCacheService releaseCacheService,
            IReleaseService releaseService)
        {
            _releaseCacheService = releaseCacheService;
            _releaseService = releaseService;
        }

        [HttpGet("publications/{publicationSlug}/releases")]
        public async Task<ActionResult<List<ReleaseSummaryViewModel>>> ListReleases(string publicationSlug)
        {
            return await _releaseService.List(publicationSlug)
                .HandleFailuresOrOk();
        }

        [MemoryCache(typeof(GetLatestReleaseCacheKey), durationInSeconds: 10, expiryScheduleCron: HalfHourlyExpirySchedule)]
        [HttpGet("publications/{publicationSlug}/releases/latest")]
        public async Task<ActionResult<ReleaseViewModel>> GetLatestRelease(string publicationSlug)
        {
            return await _releaseCacheService.GetReleaseAndPublication(publicationSlug)
                .HandleFailuresOrOk();
        }

        [HttpGet("publications/{publicationSlug}/releases/latest/summary")]
        public async Task<ActionResult<ReleaseSummaryViewModel>> GetLatestReleaseSummary(string publicationSlug)
        {
            return await _releaseCacheService.GetReleaseSummary(publicationSlug)
                .HandleFailuresOrOk();
        }

        [MemoryCache(typeof(GetReleaseCacheKey), durationInSeconds: 15, expiryScheduleCron: HalfHourlyExpirySchedule)]
        [HttpGet("publications/{publicationSlug}/releases/{releaseSlug}")]
        public async Task<ActionResult<ReleaseViewModel>> GetRelease(string publicationSlug, string releaseSlug)
        {
            return await _releaseCacheService.GetReleaseAndPublication(
                    publicationSlug,
                    releaseSlug)
                .HandleFailuresOrOk();
        }

        [HttpGet("publications/{publicationSlug}/releases/{releaseSlug}/summary")]
        public async Task<ActionResult<ReleaseSummaryViewModel>> GetReleaseSummary(string publicationSlug,
            string releaseSlug)
        {
            return await _releaseCacheService.GetReleaseSummary(
                    publicationSlug,
                    releaseSlug)
                .HandleFailuresOrOk();
        }
    }
}

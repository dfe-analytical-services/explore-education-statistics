#nullable enable
using System.Net.Mime;
using GovUk.Education.ExploreEducationStatistics.Common.Extensions;
using GovUk.Education.ExploreEducationStatistics.Common.Model;
using GovUk.Education.ExploreEducationStatistics.Common.Services.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Content.Api.Cache;
using GovUk.Education.ExploreEducationStatistics.Content.Services.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Content.Services.Interfaces.Cache;
using GovUk.Education.ExploreEducationStatistics.Content.ViewModels;
using Microsoft.AspNetCore.Mvc;
using static GovUk.Education.ExploreEducationStatistics.Common.Cache.CronSchedules;

namespace GovUk.Education.ExploreEducationStatistics.Content.Api.Controllers;

[Route("api")]
[Produces(MediaTypeNames.Application.Json)]
public class ReleaseController(
    IMethodologyCacheService methodologyCacheService,
    IPublicationCacheService publicationCacheService,
    IReleaseCacheService releaseCacheService,
    IReleaseService releaseService,
    IMemoryCacheService memoryCacheService,
    ILogger<ReleaseController> logger,
    TimeProvider timeProvider
) : ControllerBase
{
    [HttpGet("publications/{publicationSlug}/releases")]
    public async Task<ActionResult<List<ReleaseSummaryViewModel>>> ListReleases(string publicationSlug)
    {
        return await releaseService.List(publicationSlug).HandleFailuresOrOk();
    }

    [HttpGet("publications/{publicationSlug}/releases/latest")]
    public Task<ActionResult<ReleaseViewModel>> GetLatestRelease(string publicationSlug)
    {
        return memoryCacheService.GetOrCreateAsync(
            cacheKey: new GetLatestReleaseCacheKey(PublicationSlug: publicationSlug),
            createIfNotExistsFn: () => GetReleaseViewModel(publicationSlug).HandleFailuresOrOk(),
            durationInSeconds: 10,
            expiryScheduleCron: HalfHourlyExpirySchedule,
            timeProvider: timeProvider,
            logger: logger
        );
    }

    [HttpGet("publications/{publicationSlug}/releases/latest/summary")]
    public async Task<ActionResult<ReleaseSummaryViewModel>> GetLatestReleaseSummary(string publicationSlug)
    {
        return await GetReleaseSummaryViewModel(publicationSlug).HandleFailuresOrOk();
    }

    [HttpGet("publications/{publicationSlug}/releases/{releaseSlug}")]
    public Task<ActionResult<ReleaseViewModel>> GetRelease(string publicationSlug, string releaseSlug)
    {
        return memoryCacheService.GetOrCreateAsync(
            cacheKey: new GetReleaseCacheKey(PublicationSlug: publicationSlug, ReleaseSlug: releaseSlug),
            createIfNotExistsFn: () => GetReleaseViewModel(publicationSlug, releaseSlug).HandleFailuresOrOk(),
            durationInSeconds: 15,
            expiryScheduleCron: HalfHourlyExpirySchedule,
            timeProvider: timeProvider,
            logger: logger
        );
    }

    [HttpGet("publications/{publicationSlug}/releases/{releaseSlug}/summary")]
    public async Task<ActionResult<ReleaseSummaryViewModel>> GetReleaseSummary(
        string publicationSlug,
        string releaseSlug
    )
    {
        return await GetReleaseSummaryViewModel(publicationSlug, releaseSlug).HandleFailuresOrOk();
    }

    private Task<Either<ActionResult, ReleaseViewModel>> GetReleaseViewModel(
        string publicationSlug,
        string? releaseSlug = null
    ) =>
        publicationCacheService
            .GetPublication(publicationSlug)
            .OnSuccessCombineWith(publication => methodologyCacheService.GetSummariesByPublication(publication.Id))
            .OnSuccessCombineWith(_ => releaseCacheService.GetRelease(publicationSlug, releaseSlug))
            .OnSuccess(tuple =>
            {
                var (publication, methodologySummaries, release) = tuple;
                return new ReleaseViewModel(release, new PublicationViewModel(publication, methodologySummaries));
            });

    private Task<Either<ActionResult, ReleaseSummaryViewModel>> GetReleaseSummaryViewModel(
        string publicationSlug,
        string? releaseSlug = null
    )
    {
        return publicationCacheService
            .GetPublication(publicationSlug)
            .OnSuccessCombineWith(_ => releaseCacheService.GetRelease(publicationSlug, releaseSlug))
            .OnSuccess(publicationAndRelease =>
            {
                var (publication, release) = publicationAndRelease;
                return new ReleaseSummaryViewModel(release, publication);
            });
    }
}

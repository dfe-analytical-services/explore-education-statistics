#nullable enable
using System;
using System.Linq;
using System.Threading.Tasks;
using GovUk.Education.ExploreEducationStatistics.Common.Cache;
using GovUk.Education.ExploreEducationStatistics.Common.Model;
using GovUk.Education.ExploreEducationStatistics.Content.Services.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Content.Services.Interfaces.Cache;
using GovUk.Education.ExploreEducationStatistics.Content.Services.ViewModels;
using Microsoft.AspNetCore.Mvc;

namespace GovUk.Education.ExploreEducationStatistics.Content.Services.Cache;

public class ReleaseCacheService : IReleaseCacheService
{
    private readonly IMethodologyCacheService _methodologyCacheService;
    private readonly IPublicationCacheService _publicationCacheService;
    private readonly IReleaseService _releaseService;

    public ReleaseCacheService(
        IMethodologyCacheService methodologyCacheService,
        IPublicationCacheService publicationCacheService,
        IReleaseService releaseService)
    {
        _methodologyCacheService = methodologyCacheService;
        _publicationCacheService = publicationCacheService;
        _releaseService = releaseService;
    }

    [BlobCache(typeof(ReleaseCacheKey), ServiceName = "public")]
    public Task<Either<ActionResult, ReleaseCacheViewModel>> GetRelease(string publicationSlug,
        string? releaseSlug = null)
    {
        return _releaseService.GetRelease(publicationSlug, releaseSlug);
    }

    public Task<Either<ActionResult, ReleaseSummaryViewModel>> GetReleaseSummary(string publicationSlug,
        string? releaseSlug = null)
    {
        return _publicationCacheService.GetPublication(publicationSlug)
            .OnSuccessCombineWith(_ => GetRelease(publicationSlug, releaseSlug))
            .OnSuccess(publicationAndRelease =>
            {
                var (publication, release) = publicationAndRelease;
                return new ReleaseSummaryViewModel(
                    release,
                    publication
                );
            });
    }

    public Task<Either<ActionResult, ReleaseViewModel>> GetReleaseAndPublication(string publicationSlug,
        string? releaseSlug = null)
    {
        return _publicationCacheService.GetPublication(publicationSlug)
            .OnSuccessCombineWith(publication => _methodologyCacheService.GetSummariesByPublication(publication.Id))
            .OnSuccess(async publicationAndMethodologies =>
            {
                var (publication, methodologies) = publicationAndMethodologies;
                return await GetRelease(publicationSlug, releaseSlug)
                    .OnSuccess(release => new ReleaseViewModel(
                        release,
                        new PublicationViewModel(publication with
                        {
                            Releases = publication.Releases
                                .Where(releaseTitleViewModel => releaseTitleViewModel.Id != release.Id)
                                .ToList()
                        }, methodologies)));
            });
    }

    [BlobCache(typeof(ReleaseCacheKey), forceUpdate: true, ServiceName = "public")]
    public Task<Either<ActionResult, ReleaseCacheViewModel>> UpdateRelease(
        bool staging,
        DateTime expectedPublishDate,
        Guid releaseId,
        string publicationSlug,
        string? releaseSlug = null)
    {
        return _releaseService.GetRelease(releaseId, expectedPublishDate);
    }
}

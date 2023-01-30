#nullable enable
using System;
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
    private readonly IReleaseService _releaseService;

    public ReleaseCacheService(
        IReleaseService releaseService)
    {
        _releaseService = releaseService;
    }

    [BlobCache(typeof(ReleaseCacheKey), ServiceName = "public")]
    public Task<Either<ActionResult, ReleaseCacheViewModel>> GetRelease(string publicationSlug,
        string? releaseSlug = null)
    {
        return _releaseService.GetRelease(publicationSlug, releaseSlug);
    }

    [BlobCache(typeof(ReleaseCacheKey), forceUpdate: true, ServiceName = "public")]
    public Task<Either<ActionResult, ReleaseCacheViewModel>> UpdateRelease(
        Guid releaseId,
        string publicationSlug,
        string? releaseSlug = null)
    {
        return _releaseService.GetRelease(releaseId);
    }

    [BlobCache(typeof(ReleaseStagedCacheKey), forceUpdate: true, ServiceName = "public")]
    public Task<Either<ActionResult, ReleaseCacheViewModel>> UpdateReleaseStaged(
        Guid releaseId,
        DateTime expectedPublishDate,
        string publicationSlug,
        string? releaseSlug = null)
    {
        return _releaseService.GetRelease(releaseId, expectedPublishDate);
    }
}

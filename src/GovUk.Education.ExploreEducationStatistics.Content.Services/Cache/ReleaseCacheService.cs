#nullable enable
using System;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using GovUk.Education.ExploreEducationStatistics.Common;
using GovUk.Education.ExploreEducationStatistics.Common.Cache;
using GovUk.Education.ExploreEducationStatistics.Common.Extensions;
using GovUk.Education.ExploreEducationStatistics.Common.Model;
using GovUk.Education.ExploreEducationStatistics.Common.Services.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Content.Services.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Content.Services.Interfaces.Cache;
using GovUk.Education.ExploreEducationStatistics.Content.ViewModels;
using Microsoft.AspNetCore.Mvc;

namespace GovUk.Education.ExploreEducationStatistics.Content.Services.Cache;

public class ReleaseCacheService(
    IReleaseService releaseService,
    IPublicBlobStorageService publicBlobStorageService) : IReleaseCacheService
{
    private readonly IReleaseService _releaseService = releaseService;

    [BlobCache(typeof(ReleaseCacheKey), ServiceName = "public")]
    public Task<Either<ActionResult, ReleaseCacheViewModel>> GetRelease(string publicationSlug,
        string? releaseSlug = null)
    {
        return _releaseService.GetRelease(publicationSlug, releaseSlug);
    }

    [BlobCache(typeof(ReleaseCacheKey), forceUpdate: true, ServiceName = "public")]
    public Task<Either<ActionResult, ReleaseCacheViewModel>> UpdateRelease(
        Guid releaseVersionId,
        string publicationSlug,
        string? releaseSlug = null)
    {
        return _releaseService.GetRelease(releaseVersionId);
    }

    [BlobCache(typeof(ReleaseStagedCacheKey), forceUpdate: true, ServiceName = "public")]
    public Task<Either<ActionResult, ReleaseCacheViewModel>> UpdateReleaseStaged(
        Guid releaseVersionId,
        DateTime expectedPublishDate,
        string publicationSlug,
        string? releaseSlug = null)
    {
        return _releaseService.GetRelease(releaseVersionId, expectedPublishDate);
    }

    public async Task<Either<ActionResult, Unit>> RemoveRelease(
        string publicationSlug,
        string releaseSlug)
    {
        await publicBlobStorageService.DeleteBlobs(
            containerName: BlobContainers.PublicContent,
            options: new IBlobStorageService.DeleteBlobsOptions
            {
                IncludeRegex = new Regex($"^publications/{publicationSlug.TrimToLower()}/releases/{releaseSlug.TrimToLower()}.json")
            }
        );

        return Unit.Instance;
    }
}

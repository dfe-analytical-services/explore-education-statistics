using GovUk.Education.ExploreEducationStatistics.Common;
using GovUk.Education.ExploreEducationStatistics.Common.Extensions;
using GovUk.Education.ExploreEducationStatistics.Common.Model;
using GovUk.Education.ExploreEducationStatistics.Common.Services;
using GovUk.Education.ExploreEducationStatistics.Common.Services.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Content.Services.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Content.Services.Interfaces.Cache;
using GovUk.Education.ExploreEducationStatistics.Content.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace GovUk.Education.ExploreEducationStatistics.Content.Services.Cache;

public class ReleaseCacheService(
    IReleaseService releaseService,
    IPublicBlobStorageService publicBlobStorageService,
    IPublicBlobCacheService publicBlobCacheService,
    ILogger<ReleaseCacheService> logger
) : IReleaseCacheService
{
    public Task<Either<ActionResult, ReleaseCacheViewModel>> GetRelease(
        string publicationSlug,
        string? releaseSlug = null
    )
    {
        return publicBlobCacheService.GetOrCreateAsync(
            cacheKey: new ReleaseCacheKey(publicationSlug: publicationSlug, releaseSlug: releaseSlug),
            createIfNotExistsFn: () => releaseService.GetRelease(publicationSlug, releaseSlug),
            logger: logger
        );
    }

    public Task<Either<ActionResult, ReleaseCacheViewModel>> UpdateRelease(
        Guid releaseVersionId,
        string publicationSlug,
        string? releaseSlug = null
    )
    {
        return publicBlobCacheService.Update(
            cacheKey: new ReleaseCacheKey(publicationSlug: publicationSlug, releaseSlug: releaseSlug),
            createIfNotExistsFn: () => releaseService.GetRelease(releaseVersionId),
            logger: logger
        );
    }

    public Task<Either<ActionResult, ReleaseCacheViewModel>> UpdateReleaseStaged(
        Guid releaseVersionId,
        DateTime expectedPublishDate,
        string publicationSlug,
        string? releaseSlug = null
    )
    {
        return publicBlobCacheService.Update(
            cacheKey: new ReleaseStagedCacheKey(publicationSlug: publicationSlug, releaseSlug: releaseSlug),
            createIfNotExistsFn: () => releaseService.GetRelease(releaseVersionId, expectedPublishDate),
            logger: logger
        );
    }

    public async Task<Either<ActionResult, Unit>> RemoveRelease(string publicationSlug, string releaseSlug)
    {
        await publicBlobStorageService.DeleteBlob(
            containerName: BlobContainers.PublicContent,
            path: FileStoragePathUtils.PublicContentReleasePath(
                publicationSlug: publicationSlug,
                releaseSlug: releaseSlug
            )
        );

        await publicBlobStorageService.DeleteBlobs(
            containerName: BlobContainers.PublicContent,
            directoryPath: FileStoragePathUtils.PublicContentReleaseParentPath(
                publicationSlug: publicationSlug,
                releaseSlug: releaseSlug
            )
        );

        return Unit.Instance;
    }
}

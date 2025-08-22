#nullable enable
using GovUk.Education.ExploreEducationStatistics.Common;
using GovUk.Education.ExploreEducationStatistics.Common.Cache;
using GovUk.Education.ExploreEducationStatistics.Common.Model;
using GovUk.Education.ExploreEducationStatistics.Common.Services;
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
    [BlobCache(typeof(ReleaseCacheKey), ServiceName = "public")]
    public Task<Either<ActionResult, ReleaseCacheViewModel>> GetRelease(string publicationSlug,
        string? releaseSlug = null)
    {
        return releaseService.GetRelease(publicationSlug, releaseSlug);
    }

    [BlobCache(typeof(ReleaseCacheKey), forceUpdate: true, ServiceName = "public")]
    public Task<Either<ActionResult, ReleaseCacheViewModel>> UpdateRelease(
        Guid releaseVersionId,
        string publicationSlug,
        string? releaseSlug = null)
    {
        return releaseService.GetRelease(releaseVersionId);
    }

    [BlobCache(typeof(ReleaseStagedCacheKey), forceUpdate: true, ServiceName = "public")]
    public Task<Either<ActionResult, ReleaseCacheViewModel>> UpdateReleaseStaged(
        Guid releaseVersionId,
        DateTime expectedPublishDate,
        string publicationSlug,
        string? releaseSlug = null)
    {
        return releaseService.GetRelease(releaseVersionId, expectedPublishDate);
    }

    public async Task<Either<ActionResult, Unit>> RemoveRelease(
        string publicationSlug,
        string releaseSlug)
    {
        await publicBlobStorageService.DeleteBlob(
            containerName: BlobContainers.PublicContent,
            path: FileStoragePathUtils.PublicContentReleasePath(
                publicationSlug: publicationSlug,
                releaseSlug: releaseSlug)
        );

        await publicBlobStorageService.DeleteBlobs(
            containerName: BlobContainers.PublicContent,
            directoryPath: FileStoragePathUtils.PublicContentReleaseParentPath(
                publicationSlug: publicationSlug,
                releaseSlug: releaseSlug)
        );

        return Unit.Instance;
    }
}

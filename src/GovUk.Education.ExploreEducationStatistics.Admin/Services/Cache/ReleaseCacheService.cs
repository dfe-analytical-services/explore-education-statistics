#nullable enable
using GovUk.Education.ExploreEducationStatistics.Admin.Services.Interfaces.Cache;
using GovUk.Education.ExploreEducationStatistics.Common;
using GovUk.Education.ExploreEducationStatistics.Common.Services;
using GovUk.Education.ExploreEducationStatistics.Common.Services.Interfaces;

namespace GovUk.Education.ExploreEducationStatistics.Admin.Services.Cache;

public class ReleaseCacheService(IPublicBlobStorageService publicBlobStorageService) : IReleaseCacheService
{
    public async Task RemoveRelease(string publicationSlug, string releaseSlug)
    {
        await publicBlobStorageService.DeleteBlobs(
            containerName: BlobContainers.PublicContent,
            directoryPath: FileStoragePathUtils.PublicContentReleaseParentPath(
                publicationSlug: publicationSlug,
                releaseSlug: releaseSlug
            )
        );
    }
}

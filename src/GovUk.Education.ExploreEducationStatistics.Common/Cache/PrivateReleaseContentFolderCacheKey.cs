#nullable enable
using GovUk.Education.ExploreEducationStatistics.Common.Cache.Interfaces;
using static GovUk.Education.ExploreEducationStatistics.Common.BlobContainers;
using static GovUk.Education.ExploreEducationStatistics.Common.Services.FileStoragePathUtils;

namespace GovUk.Education.ExploreEducationStatistics.Common.Cache;

public record PrivateReleaseContentFolderCacheKey : IBlobCacheKey
{
    private Guid ReleaseVersionId { get; }

    public PrivateReleaseContentFolderCacheKey(Guid releaseVersionId)
    {
        ReleaseVersionId = releaseVersionId;
    }

    public string Key => $"{ReleasesDirectory}/{ReleaseVersionId}";

    public IBlobContainer Container => PrivateContent;
}

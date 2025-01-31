#nullable enable
using GovUk.Education.ExploreEducationStatistics.Common;
using GovUk.Education.ExploreEducationStatistics.Common.Cache.Interfaces;
using static GovUk.Education.ExploreEducationStatistics.Common.Services.FileStoragePathUtils;

namespace GovUk.Education.ExploreEducationStatistics.Content.Services.Cache;

public record ReleaseStagedCacheKey : IBlobCacheKey
{
    private string PublicationSlug { get; }

    private string? ReleaseSlug { get; }

    public ReleaseStagedCacheKey(string publicationSlug, string? releaseSlug = null)
    {
        PublicationSlug = publicationSlug;
        ReleaseSlug = releaseSlug;
    }

    public IBlobContainer Container => BlobContainers.PublicContent;

    public string Key => ReleaseSlug == null
        ? PublicContentLatestReleasePath(PublicationSlug, staging: true)
        : PublicContentReleasePath(PublicationSlug, ReleaseSlug, staging: true);
}

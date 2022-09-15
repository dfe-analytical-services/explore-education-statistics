#nullable enable
using GovUk.Education.ExploreEducationStatistics.Common;
using GovUk.Education.ExploreEducationStatistics.Common.Cache.Interfaces;
using static GovUk.Education.ExploreEducationStatistics.Common.Services.FileStoragePathUtils;

namespace GovUk.Education.ExploreEducationStatistics.Content.Services.Cache;

public record ReleaseCacheKey : IBlobCacheKey
{
    private bool Staging { get; }

    private string PublicationSlug { get; }

    private string? ReleaseSlug { get; }

    public ReleaseCacheKey(string publicationSlug, string? releaseSlug = null)
    {
        PublicationSlug = publicationSlug;
        ReleaseSlug = releaseSlug;
    }

    public ReleaseCacheKey(bool staging, string publicationSlug, string? releaseSlug = null)
    {
        Staging = staging;
        PublicationSlug = publicationSlug;
        ReleaseSlug = releaseSlug;
    }

    public IBlobContainer Container => BlobContainers.PublicContent;

    public string Key => ReleaseSlug == null
        ? PublicContentLatestReleasePath(PublicationSlug, Staging)
        : PublicContentReleasePath(PublicationSlug, ReleaseSlug, Staging);
}

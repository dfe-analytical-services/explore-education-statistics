#nullable enable
using GovUk.Education.ExploreEducationStatistics.Common;
using GovUk.Education.ExploreEducationStatistics.Common.Cache.Interfaces;
using static GovUk.Education.ExploreEducationStatistics.Common.Services.FileStoragePathUtils;

namespace GovUk.Education.ExploreEducationStatistics.Data.Api.Services.Cache;

public record ReleaseSubjectsCacheKey : IBlobCacheKey
{
    private string PublicationSlug { get; }
    private string ReleaseSlug { get; }

    public Guid ReleaseVersionId { get; }

    public ReleaseSubjectsCacheKey(
        string publicationSlug,
        string releaseSlug,
        Guid releaseVersionId
    )
    {
        PublicationSlug = publicationSlug;
        ReleaseSlug = releaseSlug;
        ReleaseVersionId = releaseVersionId;
    }

    public ReleaseSubjectsCacheKey(ReleaseSubjectsCacheKey cacheKey)
    {
        PublicationSlug = cacheKey.PublicationSlug;
        ReleaseSlug = cacheKey.ReleaseSlug;
        ReleaseVersionId = cacheKey.ReleaseVersionId;
    }

    public IBlobContainer Container => BlobContainers.PublicContent;

    public string Key => PublicContentReleaseSubjectsPath(PublicationSlug, ReleaseSlug);
}

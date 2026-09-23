#nullable enable
using GovUk.Education.ExploreEducationStatistics.Common;
using GovUk.Education.ExploreEducationStatistics.Common.Cache.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Content.Model;
using static GovUk.Education.ExploreEducationStatistics.Common.Services.FileStoragePathUtils;

namespace GovUk.Education.ExploreEducationStatistics.Data.Api.Cache;

public record DataBlockTableResultCacheKey : IBlobCacheKey
{
    private string PublicationSlug { get; }
    private string ReleaseSlug { get; }
    private Guid DataBlockId { get; }

    // ReSharper disable once UnusedMember.Global
    public DataBlockTableResultCacheKey(DataBlockVersion dataBlockVersion)
        : this(
            publicationSlug: dataBlockVersion.ReleaseVersion.Release.Publication.Slug,
            releaseSlug: dataBlockVersion.ReleaseVersion.Release.Slug,
            dataBlockId: dataBlockVersion.DataBlockId
        ) { }

    public DataBlockTableResultCacheKey(string publicationSlug, string releaseSlug, Guid dataBlockId)
    {
        PublicationSlug = publicationSlug;
        ReleaseSlug = releaseSlug;
        DataBlockId = dataBlockId;
    }

    public IBlobContainer Container => BlobContainers.PublicContent;

    public string Key => PublicContentDataBlockVersionPath(PublicationSlug, ReleaseSlug, DataBlockId);
}

#nullable enable
using GovUk.Education.ExploreEducationStatistics.Common;
using GovUk.Education.ExploreEducationStatistics.Common.Cache.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Content.Model;
using static GovUk.Education.ExploreEducationStatistics.Common.Services.FileStoragePathUtils;

namespace GovUk.Education.ExploreEducationStatistics.Data.Api.Cache;

public record LocationsForDataBlockCacheKey : IBlobCacheKey
{
    private string PublicationSlug { get; }
    private string ReleaseSlug { get; }
    private Guid DataBlockId { get; }
    private long BoundaryLevelId { get; }

    public LocationsForDataBlockCacheKey(DataBlockVersion dataBlockVersion, long boundaryLevelId)
        : this(
            publicationSlug: dataBlockVersion.ReleaseVersion.Release.Publication.Slug,
            releaseSlug: dataBlockVersion.ReleaseVersion.Release.Slug,
            dataBlockId: dataBlockVersion.DataBlockId,
            boundaryLevelId: boundaryLevelId
        ) { }

    public LocationsForDataBlockCacheKey(
        string publicationSlug,
        string releaseSlug,
        Guid dataBlockId,
        long boundaryLevelId
    )
    {
        PublicationSlug = publicationSlug;
        ReleaseSlug = releaseSlug;
        DataBlockId = dataBlockId;
        BoundaryLevelId = boundaryLevelId;
    }

    public IBlobContainer Container => BlobContainers.PublicContent;

    public string Key =>
        PublicContentDataBlockLocationsPath(PublicationSlug, ReleaseSlug, DataBlockId, BoundaryLevelId);
}

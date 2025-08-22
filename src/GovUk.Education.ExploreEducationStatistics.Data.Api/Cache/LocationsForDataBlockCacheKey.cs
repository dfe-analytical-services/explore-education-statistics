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
    private Guid DataBlockParentId { get; }
    private long BoundaryLevelId { get; }

    public LocationsForDataBlockCacheKey(DataBlockVersion dataBlockVersion, long boundaryLevelId) : this(
        publicationSlug: dataBlockVersion.ReleaseVersion.Release.Publication.Slug,
        releaseSlug: dataBlockVersion.ReleaseVersion.Release.Slug,
        dataBlockParentId: dataBlockVersion.DataBlockParentId,
        boundaryLevelId: boundaryLevelId)
    {
    }

    public LocationsForDataBlockCacheKey(
        string publicationSlug,
        string releaseSlug,
        Guid dataBlockParentId,
        long boundaryLevelId)
    {
        PublicationSlug = publicationSlug;
        ReleaseSlug = releaseSlug;
        DataBlockParentId = dataBlockParentId;
        BoundaryLevelId = boundaryLevelId;
    }

    public IBlobContainer Container => BlobContainers.PublicContent;

    public string Key => PublicContentDataBlockLocationsPath(
        PublicationSlug,
        ReleaseSlug,
        DataBlockParentId,
        BoundaryLevelId
    );
}

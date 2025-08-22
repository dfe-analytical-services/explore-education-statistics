#nullable enable
using GovUk.Education.ExploreEducationStatistics.Common;
using GovUk.Education.ExploreEducationStatistics.Common.Cache.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Content.Model;
using static GovUk.Education.ExploreEducationStatistics.Common.Services.FileStoragePathUtils;

namespace GovUk.Education.ExploreEducationStatistics.Admin.Cache;

public record LocationsForDataBlockCacheKey : IBlobCacheKey
{
    private Guid ReleaseVersionId { get; }
    private Guid DataBlockId { get; }
    private long BoundaryLevelId { get; }

    public LocationsForDataBlockCacheKey(DataBlockVersion dataBlockVersion, long boundaryLevelId) : this(
        releaseVersionId: dataBlockVersion.ReleaseVersionId,
        dataBlockId: dataBlockVersion.DataBlockParentId,
        boundaryLevelId: boundaryLevelId)
    {
    }

    public LocationsForDataBlockCacheKey(
        Guid releaseVersionId,
        Guid dataBlockId,
        long boundaryLevelId)
    {
        ReleaseVersionId = releaseVersionId;
        DataBlockId = dataBlockId;
        BoundaryLevelId = boundaryLevelId;
    }

    public IBlobContainer Container => BlobContainers.PrivateContent;

    public string Key => PrivateContentDataBlockLocationsPath(
        ReleaseVersionId,
        DataBlockId,
        BoundaryLevelId
    );
}

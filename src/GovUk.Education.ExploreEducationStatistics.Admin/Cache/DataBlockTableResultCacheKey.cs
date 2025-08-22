#nullable enable
using GovUk.Education.ExploreEducationStatistics.Common;
using GovUk.Education.ExploreEducationStatistics.Common.Cache.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Content.Model;
using static GovUk.Education.ExploreEducationStatistics.Common.BlobContainers;
using static GovUk.Education.ExploreEducationStatistics.Common.Services.FileStoragePathUtils;

namespace GovUk.Education.ExploreEducationStatistics.Admin.Cache;

// EES-4640 - ideally this should be renamed to "DataBlockVersionTableResultCacheKey" when DataBlock is removed
// from the DataBlock model.
public class DataBlockTableResultCacheKey : IBlobCacheKey
{
    private Guid ReleaseVersionId { get; }
    private Guid DataBlockId { get; }

    public DataBlockTableResultCacheKey(DataBlockVersion dataBlockVersion)
    {
        ReleaseVersionId = dataBlockVersion.ReleaseVersionId;
        DataBlockId = dataBlockVersion.Id;
    }

    public IBlobContainer Container => PrivateContent;

    public string Key => PrivateContentDataBlockPath(
        ReleaseVersionId,
        DataBlockId
    );
}

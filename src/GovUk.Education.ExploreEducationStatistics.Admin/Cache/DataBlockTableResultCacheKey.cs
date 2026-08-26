#nullable enable
using GovUk.Education.ExploreEducationStatistics.Common;
using GovUk.Education.ExploreEducationStatistics.Common.Cache.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Content.Model;
using static GovUk.Education.ExploreEducationStatistics.Common.BlobContainers;
using static GovUk.Education.ExploreEducationStatistics.Common.Services.FileStoragePathUtils;

namespace GovUk.Education.ExploreEducationStatistics.Admin.Cache;

public class DataBlockVersionTableResultCacheKey(DataBlockVersion dataBlockVersion) : IBlobCacheKey
{
    private Guid ReleaseVersionId { get; } = dataBlockVersion.ReleaseVersionId;
    private Guid DataBlockVersionId { get; } = dataBlockVersion.Id;

    public IBlobContainer Container => PrivateContent;

    public string Key => PrivateContentDataBlockPath(ReleaseVersionId, DataBlockVersionId);
}

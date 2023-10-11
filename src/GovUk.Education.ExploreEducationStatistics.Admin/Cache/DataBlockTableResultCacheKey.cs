#nullable enable
using System;
using GovUk.Education.ExploreEducationStatistics.Common;
using GovUk.Education.ExploreEducationStatistics.Common.Cache.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Content.Model;
using static GovUk.Education.ExploreEducationStatistics.Common.BlobContainers;
using static GovUk.Education.ExploreEducationStatistics.Common.Services.FileStoragePathUtils;

namespace GovUk.Education.ExploreEducationStatistics.Admin.Cache
{
    public class DataBlockTableResultCacheKey : IBlobCacheKey
    {
        private Guid ReleaseId { get; }
        private Guid DataBlockId { get; }

        public DataBlockTableResultCacheKey(DataBlock dataBlock)
        {
            ReleaseId = dataBlock.ReleaseId;
            DataBlockId = dataBlock.Id;
        }

        public IBlobContainer Container => PrivateContent;

        public string Key => PrivateContentDataBlockPath(
            ReleaseId,
            DataBlockId
        );
    }
}

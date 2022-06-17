#nullable enable
using System;
using GovUk.Education.ExploreEducationStatistics.Common.Cache.Interfaces;
using static GovUk.Education.ExploreEducationStatistics.Common.BlobContainers;
using static GovUk.Education.ExploreEducationStatistics.Common.Services.FileStoragePathUtils;

namespace GovUk.Education.ExploreEducationStatistics.Common.Cache
{
    public record PrivateReleaseContentFolderCacheKey : IBlobCacheKey
    {
        private Guid ReleaseId { get; }

        public PrivateReleaseContentFolderCacheKey(Guid releaseId)
        {
            ReleaseId = releaseId;
        }

        public string Key => $"{ReleasesDirectory}/{ReleaseId}";

        public IBlobContainer Container => PrivateContent;
    }
}

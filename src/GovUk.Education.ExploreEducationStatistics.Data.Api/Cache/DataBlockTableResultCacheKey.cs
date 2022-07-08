#nullable enable
using System;
using GovUk.Education.ExploreEducationStatistics.Common;
using GovUk.Education.ExploreEducationStatistics.Common.Cache.Interfaces;
using static GovUk.Education.ExploreEducationStatistics.Common.Services.FileStoragePathUtils;

namespace GovUk.Education.ExploreEducationStatistics.Data.Api.Cache
{
    public record DataBlockTableResultCacheKey : IBlobCacheKey
    {
        private string PublicationSlug { get; }
        private string ReleaseSlug { get; }
        private Guid DataBlockId { get; }

        public DataBlockTableResultCacheKey(CacheableDataBlock cacheable)
        {
            PublicationSlug = cacheable.PublicationSlug;
            ReleaseSlug = cacheable.ReleaseSlug;
            DataBlockId = cacheable.DataBlockId;
        }

        public DataBlockTableResultCacheKey(string publicationSlug, string releaseSlug, Guid dataBlockId)
        {
            PublicationSlug = publicationSlug;
            ReleaseSlug = releaseSlug;
            DataBlockId = dataBlockId;
        }

        public IBlobContainer Container => BlobContainers.PublicContent;

        public string Key => PublicContentDataBlockPath(
            PublicationSlug,
            ReleaseSlug,
            DataBlockId
        );
    }
}

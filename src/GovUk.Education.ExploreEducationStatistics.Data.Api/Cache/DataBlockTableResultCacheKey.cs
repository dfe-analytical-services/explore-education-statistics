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
        // TODO DW - will this now become FastTrackId?
        private Guid DataBlockId { get; }

        public DataBlockTableResultCacheKey(CacheableFastTrack cacheable)
        {
            PublicationSlug = cacheable.PublicationSlug;
            ReleaseSlug = cacheable.ReleaseSlug;
            DataBlockId = cacheable.FastTrackId;
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

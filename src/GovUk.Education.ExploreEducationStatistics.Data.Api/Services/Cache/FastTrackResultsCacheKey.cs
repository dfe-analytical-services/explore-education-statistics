#nullable enable
using System;
using GovUk.Education.ExploreEducationStatistics.Common;
using GovUk.Education.ExploreEducationStatistics.Common.Cache.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Content.Model;
using static GovUk.Education.ExploreEducationStatistics.Common.Services.FileStoragePathUtils;

namespace GovUk.Education.ExploreEducationStatistics.Data.Api.Services.Cache
{
    public record FastTrackResultsCacheKey : IBlobCacheKey
    {
        private string PublicationSlug { get; }
        private string ReleaseSlug { get; }
        public Guid FastTrackId { get; }

        public FastTrackResultsCacheKey(string publicationSlug, string releaseSlug, Guid fastTrackId)
        {
            PublicationSlug = publicationSlug;
            ReleaseSlug = releaseSlug;
            FastTrackId = fastTrackId;
        }

        public FastTrackResultsCacheKey(FastTrackResultsCacheKey cacheKey)
        {
            PublicationSlug = cacheKey.PublicationSlug;
            ReleaseSlug = cacheKey.ReleaseSlug;
            FastTrackId = cacheKey.FastTrackId;
        }
        
        public IBlobContainer Container => BlobContainers.PublicContent;

        public string Key => PublicContentFastTrackResultsPath(PublicationSlug, ReleaseSlug, FastTrackId);
    }
}

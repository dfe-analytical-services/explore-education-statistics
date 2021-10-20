#nullable enable
using System;
using GovUk.Education.ExploreEducationStatistics.Common;
using GovUk.Education.ExploreEducationStatistics.Common.Cache.Interfaces;
using static GovUk.Education.ExploreEducationStatistics.Common.Services.FileStoragePathUtils;

namespace GovUk.Education.ExploreEducationStatistics.Data.Api.Controllers.Cache
{
    public record FastTrackResultsCacheKey : IBlobCacheKey
    {
        private Guid FastTrackId { get; }

        public FastTrackResultsCacheKey(string fastTrackId)
        {
            FastTrackId = Guid.Parse(fastTrackId);
        }
        
        public IBlobContainer Container => BlobContainers.PublicContent;

        public string Key => PublicContentFastTrackResultsPath(FastTrackId);
    }
}

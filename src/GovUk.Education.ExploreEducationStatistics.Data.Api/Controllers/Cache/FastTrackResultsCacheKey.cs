#nullable enable
using System;
using GovUk.Education.ExploreEducationStatistics.Common;
using GovUk.Education.ExploreEducationStatistics.Common.Cache.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Data.Model;
using static GovUk.Education.ExploreEducationStatistics.Common.Services.FileStoragePathUtils;

namespace GovUk.Education.ExploreEducationStatistics.Data.Api.Controllers.Cache
{
    public record FastTrackResultsCacheKey : IBlobCacheKey
    {
        private Guid ReleaseId { get; }
        private Guid FastTrackId { get; }

        public FastTrackResultsCacheKey(ReleaseFastTrack fastTrack)
        {
            ReleaseId = fastTrack.ReleaseId;
            FastTrackId = fastTrack.FastTrackId;
        }
        
        public IBlobContainer Container => BlobContainers.PublicContent;

        public string Key => PublicContentFastTrackResultsPath(ReleaseId, FastTrackId);
    }
}

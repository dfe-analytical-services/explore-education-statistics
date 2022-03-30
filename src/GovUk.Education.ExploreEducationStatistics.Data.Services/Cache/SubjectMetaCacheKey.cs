#nullable enable
using System;
using GovUk.Education.ExploreEducationStatistics.Common;
using GovUk.Education.ExploreEducationStatistics.Common.Cache.Interfaces;
using static GovUk.Education.ExploreEducationStatistics.Common.Services.FileStoragePathUtils;

namespace GovUk.Education.ExploreEducationStatistics.Data.Services.Cache
{
    public record SubjectMetaCacheKey : IBlobCacheKey
    {
        private string PublicationSlug { get; }
        private string ReleaseSlug { get; }
        public Guid SubjectId { get; }

        public SubjectMetaCacheKey(string publicationSlug, string releaseSlug, Guid subjectId)
        {
            PublicationSlug = publicationSlug;
            ReleaseSlug = releaseSlug;
            SubjectId = subjectId;
        }

        public SubjectMetaCacheKey(SubjectMetaCacheKey cacheKey)
        {
            PublicationSlug = cacheKey.PublicationSlug;
            ReleaseSlug = cacheKey.ReleaseSlug;
            SubjectId = cacheKey.SubjectId;
        }
        
        public IBlobContainer Container => BlobContainers.PublicContent;

        public string Key => PublicContentSubjectMetaPath(PublicationSlug, ReleaseSlug, SubjectId);
    }
}

#nullable enable
using System;
using GovUk.Education.ExploreEducationStatistics.Common;
using GovUk.Education.ExploreEducationStatistics.Common.Cache.Interfaces;
using static GovUk.Education.ExploreEducationStatistics.Common.Services.FileStoragePathUtils;

namespace GovUk.Education.ExploreEducationStatistics.Data.Api.Cache
{
    public record SubjectMetaCacheKey : IBlobCacheKey
    {
        private string PublicationSlug { get; }
        private string ReleaseSlug { get; }
        private Guid SubjectId { get; }

        public SubjectMetaCacheKey(CacheableReleaseSubject cacheable)
        {
            PublicationSlug = cacheable.PublicationSlug;
            ReleaseSlug = cacheable.ReleaseSlug;
            SubjectId = cacheable.SubjectId;
        }

        public SubjectMetaCacheKey(string publicationSlug, string releaseSlug, Guid subjectId)
        {
            PublicationSlug = publicationSlug;
            ReleaseSlug = releaseSlug;
            SubjectId = subjectId;
        }

        public IBlobContainer Container => BlobContainers.PublicContent;

        public string Key => PublicContentSubjectMetaPath(PublicationSlug, ReleaseSlug, SubjectId);
    }
}

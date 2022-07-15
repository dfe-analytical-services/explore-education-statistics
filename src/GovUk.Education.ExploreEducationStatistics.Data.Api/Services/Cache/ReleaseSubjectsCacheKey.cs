#nullable enable
using System;
using GovUk.Education.ExploreEducationStatistics.Common;
using GovUk.Education.ExploreEducationStatistics.Common.Cache.Interfaces;
using static GovUk.Education.ExploreEducationStatistics.Common.Services.FileStoragePathUtils;

namespace GovUk.Education.ExploreEducationStatistics.Data.Api.Services.Cache
{
    public record ReleaseSubjectsCacheKey : IBlobCacheKey
    {
        private string PublicationSlug { get; }
        private string ReleaseSlug { get; }

        public Guid ReleaseId { get; }

        public ReleaseSubjectsCacheKey(string publicationSlug, string releaseSlug, Guid releaseId)
        {
            PublicationSlug = publicationSlug;
            ReleaseSlug = releaseSlug;
            ReleaseId = releaseId;
        }

        public ReleaseSubjectsCacheKey(ReleaseSubjectsCacheKey cacheKey)
        {
            PublicationSlug = cacheKey.PublicationSlug;
            ReleaseSlug = cacheKey.ReleaseSlug;
            ReleaseId = cacheKey.ReleaseId;
        }

        public IBlobContainer Container => BlobContainers.PublicContent;

        public string Key => PublicContentReleaseSubjectsPath(PublicationSlug, ReleaseSlug);
    }
}

#nullable enable
using GovUk.Education.ExploreEducationStatistics.Common;
using GovUk.Education.ExploreEducationStatistics.Common.Cache.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Common.Services;

namespace GovUk.Education.ExploreEducationStatistics.Content.Services.Cache
{
    public record PublicationCacheKey(string PublicationSlug) : IBlobCacheKey
    {
        private string PublicationSlug { get; } = PublicationSlug;

        public string Key => FileStoragePathUtils.PublicContentPublicationPath(PublicationSlug);

        public IBlobContainer Container => BlobContainers.PublicContent;
    }
}

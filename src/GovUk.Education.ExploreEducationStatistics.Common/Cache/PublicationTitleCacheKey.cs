#nullable enable
using GovUk.Education.ExploreEducationStatistics.Common;
using GovUk.Education.ExploreEducationStatistics.Common.Cache.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Common.Services;

namespace GovUk.Education.ExploreEducationStatistics.Content.Api.Cache
{
    public record PublicationTitleCacheKey(string PublicationSlug) : IBlobCacheKey
    {
        private string PublicationSlug { get; } = PublicationSlug;

        public string Key => FileStoragePathUtils.PublicContentPublicationTitlePath(PublicationSlug);

        public IBlobContainer Container => BlobContainers.PublicContent;
    }
}

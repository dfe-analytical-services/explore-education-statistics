#nullable enable
using GovUk.Education.ExploreEducationStatistics.Common.Cache.Interfaces;

namespace GovUk.Education.ExploreEducationStatistics.Common.Cache
{
    public record PublicationDownloadsTreeCacheKey : IBlobCacheKey
    {
        public string Key => "publication-downloads-tree.json";

        public IBlobContainer Container => BlobContainers.PublicContent;
    }
}
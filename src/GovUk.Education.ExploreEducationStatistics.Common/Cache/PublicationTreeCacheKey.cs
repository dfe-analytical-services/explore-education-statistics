#nullable enable
using GovUk.Education.ExploreEducationStatistics.Common.Cache.Interfaces;

namespace GovUk.Education.ExploreEducationStatistics.Common.Cache
{
    public record PublicationTreeCacheKey : IBlobCacheKey
    {
        public string Key => "publication-tree.json";

        public IBlobContainer Container => BlobContainers.PublicContent;
    }
}
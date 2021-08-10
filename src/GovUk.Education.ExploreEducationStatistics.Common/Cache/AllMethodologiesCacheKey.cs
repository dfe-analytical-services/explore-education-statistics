#nullable enable
using GovUk.Education.ExploreEducationStatistics.Common.Cache.Interfaces;

namespace GovUk.Education.ExploreEducationStatistics.Common.Cache
{
    public record AllMethodologiesCacheKey : IBlobCacheKey
    {
        public string Key => "methodology-tree.json";

        public IBlobContainer Container => BlobContainers.PublicContent;
    }
}

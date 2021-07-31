#nullable enable
using GovUk.Education.ExploreEducationStatistics.Common.Cache.Interfaces;

namespace GovUk.Education.ExploreEducationStatistics.Common.Cache
{
    public record AllMethodologiesCacheKey : IBlobCacheKey
    {
        public string Key => "627b1bfc-3436-474c-9d10-7b0b98b6dee5";

        public IBlobContainer Container { get; }

        public AllMethodologiesCacheKey(IBlobContainer container)
        {
            Container = container;
        }
    }
}

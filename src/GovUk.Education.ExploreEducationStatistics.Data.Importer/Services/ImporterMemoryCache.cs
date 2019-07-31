using Microsoft.Extensions.Caching.Memory;

namespace GovUk.Education.ExploreEducationStatistics.Data.Importer.Services
{

    public class ImporterMemoryCache
    {
        public MemoryCache Cache { get; set; }
    
        public ImporterMemoryCache()
        {
            Cache = new MemoryCache(new MemoryCacheOptions());
        }
    }
}
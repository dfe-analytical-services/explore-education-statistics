using Microsoft.Extensions.Caching.Memory;

namespace GovUk.Education.ExploreEducationStatistics.Data.Importer.Services
{

    public class MyMemoryCache
    {
        public MemoryCache Cache { get; set; }
    
        public MyMemoryCache()
        {
            /*
            Cache = new MemoryCache(new MemoryCacheOptions
            {
                SizeLimit = 1024
            });
            */
            Cache = new MemoryCache(new MemoryCacheOptions());
        }
    }
}
using Microsoft.Extensions.Caching.Memory;

namespace GovUk.Education.ExploreEducationStatistics.Data.Importer.Services
{
    public class BaseImporterService
    {
        private readonly ImporterMemoryCache _cache;

        protected BaseImporterService(ImporterMemoryCache cache)
        {
            _cache = cache;
        }

        protected MemoryCache GetCache()
        {
            return _cache.Cache;
        }

        public void ClearCache()
        {
            _cache.ClearMemoryCache();
        }
    }
}
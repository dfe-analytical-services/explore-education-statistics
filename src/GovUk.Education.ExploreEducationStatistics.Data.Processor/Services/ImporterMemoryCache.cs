#nullable enable
using System;
using Microsoft.Extensions.Caching.Memory;

namespace GovUk.Education.ExploreEducationStatistics.Data.Processor.Services
{
    public class ImporterMemoryCache
    {
        public MemoryCache Cache { get; set; } = new(new MemoryCacheOptions());

        public void Clear()
        {
            Cache.Dispose();
            Cache = new MemoryCache(new MemoryCacheOptions());
            GC.Collect();
        }
    }
}

#nullable enable
using System.Threading.Tasks;
using GovUk.Education.ExploreEducationStatistics.Common.Cache.Interfaces;

namespace GovUk.Education.ExploreEducationStatistics.Common.Services.Interfaces
{
    public interface IBlobCacheService : ICacheService<IBlobCacheKey>
    {
        Task DeleteItemAsync(IBlobCacheKey cacheKey);
        
        Task DeleteCacheFolderAsync(IBlobCacheKey cacheFolderKey);
    }
}

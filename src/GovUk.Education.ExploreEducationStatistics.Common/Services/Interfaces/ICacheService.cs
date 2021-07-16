#nullable enable
using System;
using System.Threading.Tasks;
using GovUk.Education.ExploreEducationStatistics.Common.Model.Interfaces;

namespace GovUk.Education.ExploreEducationStatistics.Common.Services.Interfaces
{
    public interface ICacheService
    {
        public Task<TEntity> GetCachedEntity<TEntity>(ICacheKey<TEntity> cacheKey,
            Func<TEntity> entityProvider)
            where TEntity : class;

        public Task<TEntity> GetCachedEntity<TEntity>(ICacheKey<TEntity> cacheKey,
            Func<Task<TEntity>> entityProvider)
            where TEntity : class;
    }
}

#nullable enable
using System;
using System.Threading.Tasks;
using GovUk.Education.ExploreEducationStatistics.Common.Model;
using GovUk.Education.ExploreEducationStatistics.Common.Model.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace GovUk.Education.ExploreEducationStatistics.Common.Services.Interfaces
{
    public interface ICacheService
    {
        public Task<TEntity> GetCachedEntity<TEntity>(
            IBlobContainer blobContainer,
            ICacheKey<TEntity> cacheKey,
            Func<TEntity> entityProvider)
            where TEntity : class;

        public Task<TEntity> GetCachedEntity<TEntity>(
            IBlobContainer blobContainer,
            ICacheKey<TEntity> cacheKey,
            Func<Task<TEntity>> entityProvider)
            where TEntity : class;

        public Task<Either<ActionResult, TEntity>> GetCachedEntity<TEntity>(
            IBlobContainer blobContainer,
            ICacheKey<TEntity> cacheKey,
            Func<Task<Either<ActionResult, TEntity>>> entityProvider)
            where TEntity : class;
    }
}

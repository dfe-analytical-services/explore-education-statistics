#nullable enable
using System;
using System.Threading.Tasks;
using GovUk.Education.ExploreEducationStatistics.Common.Cache.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Common.Model;
using Microsoft.AspNetCore.Mvc;

namespace GovUk.Education.ExploreEducationStatistics.Common.Services.Interfaces
{
    public interface ICacheService
    {
        public Task<TEntity> GetItem<TEntity>(
            IBlobContainer blobContainer,
            ICacheKey cacheKey,
            Func<TEntity> entityProvider)
            where TEntity : class;

        public Task<TEntity> GetItem<TEntity>(
            IBlobContainer blobContainer,
            ICacheKey cacheKey,
            Func<Task<TEntity>> entityProvider)
            where TEntity : class;

        public Task<Either<ActionResult, TEntity>> GetItem<TEntity>(
            IBlobContainer blobContainer,
            ICacheKey cacheKey,
            Func<Task<Either<ActionResult, TEntity>>> entityProvider)
            where TEntity : class;

        public Task DeleteItem(
            IBlobContainer blobContainer,
            ICacheKey cacheKey);
    }
}

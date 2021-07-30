#nullable enable
using System;
using System.Threading.Tasks;
using GovUk.Education.ExploreEducationStatistics.Common.Cache.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Common.Model;
using GovUk.Education.ExploreEducationStatistics.Common.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Moq;

namespace GovUk.Education.ExploreEducationStatistics.Common.Tests.Extensions
{
    public static class MockCacheServiceExtensions
    {
        /// <summary>
        /// Set up a call on a mocked cache service that always returns the result of invoking the entity provider
        /// to emulate a cache miss.
        /// </summary>
        /// <param name="cacheService"></param>
        /// <param name="expectedBlobContainer"></param>
        /// <param name="expectedCacheKey"></param>
        /// <typeparam name="TEntity"></typeparam>
        /// <returns></returns>
        public static Mock<ICacheService> SetupGetItemForCacheMiss<TEntity>(
            this Mock<ICacheService> cacheService,
            IBlobContainer expectedBlobContainer,
            ICacheKey? expectedCacheKey = null)
            where TEntity : class
        {
            cacheService.Setup(mock => mock.GetItem(
                    expectedBlobContainer,
                    It.Is<ICacheKey>(ck =>
                        expectedCacheKey == null ||
                        ck.Key == expectedCacheKey.Key),
                    It.IsAny<Func<Task<TEntity>>>()))
                .Returns(async (IBlobContainer blobContainer,
                    ICacheKey cacheKey,
                    Func<Task<TEntity>> entityProvider) => await entityProvider());

            return cacheService;
        }

        /// <summary>
        /// Set up a call on a mocked cache service that always returns the result of invoking the entity provider
        /// to emulate a cache miss.
        /// </summary>
        /// <param name="cacheService"></param>
        /// <param name="expectedBlobContainer"></param>
        /// <param name="expectedCacheKey"></param>
        /// <typeparam name="TEntity"></typeparam>
        /// <typeparam name="TProvider"></typeparam>
        /// <returns></returns>
        public static Mock<ICacheService> SetupGetItemForCacheMiss<TEntity, TProvider>(
            this Mock<ICacheService> cacheService,
            IBlobContainer expectedBlobContainer,
            ICacheKey? expectedCacheKey = null)
            where TEntity : class
            where TProvider : Task<Either<ActionResult, TEntity>>
        {
            cacheService.Setup(mock => mock.GetItem(
                    expectedBlobContainer,
                    It.Is<ICacheKey>(ck =>
                        expectedCacheKey == null ||
                        ck.Key == expectedCacheKey.Key),
                    It.IsAny<Func<Task<Either<ActionResult, TEntity>>>>()))
                .Returns(async (IBlobContainer blobContainer,
                        ICacheKey cacheKey,
                        Func<Task<Either<ActionResult, TEntity>>> entityProvider) =>
                    await entityProvider());

            return cacheService;
        }
    }
}

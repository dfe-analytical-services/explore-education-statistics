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
    public static class MockBlobCacheServiceExtensions
    {
        /// <summary>
        /// Set up a call on a mocked cache service that always returns the result of invoking the entity provider
        /// to emulate a cache miss.
        /// </summary>
        /// <param name="cacheService"></param>
        /// <param name="expectedCacheKey"></param>
        /// <typeparam name="TItem"></typeparam>
        /// <returns></returns>
        public static Mock<IBlobCacheService> SetupGetItemForCacheMiss<TItem>(
            this Mock<IBlobCacheService> cacheService,
            IBlobCacheKey expectedCacheKey)
            where TItem : class
        {
            cacheService.Setup(mock => mock.GetItem(
                    It.Is<IBlobCacheKey>(ck =>
                        ck.Key == expectedCacheKey.Key
                        && ck.Container == expectedCacheKey.Container),
                    It.IsAny<Func<Task<TItem>>>()))
                .Returns(async (
                    IBlobCacheKey _,
                    Func<Task<TItem>> entityProvider) => await entityProvider());

            return cacheService;
        }

        /// <summary>
        /// Set up a call on a mocked cache service that always returns the result of invoking the entity provider
        /// to emulate a cache miss.
        /// </summary>
        /// <param name="cacheService"></param>
        /// <param name="expectedCacheKey"></param>
        /// <typeparam name="TItem"></typeparam>
        /// <typeparam name="TSupplier"></typeparam>
        /// <returns></returns>
        public static Mock<IBlobCacheService> SetupGetItemForCacheMiss<TItem, TSupplier>(
            this Mock<IBlobCacheService> cacheService,
            IBlobCacheKey expectedCacheKey)
            where TItem : class
            where TSupplier : Task<Either<ActionResult, TItem>>
        {
            cacheService.Setup(mock => mock.GetItem(
                    It.Is<IBlobCacheKey>(ck =>
                        ck.Key == expectedCacheKey.Key
                        && ck.Container == expectedCacheKey.Container),
                    It.IsAny<Func<Task<Either<ActionResult, TItem>>>>()))
                .Returns(async (
                        IBlobCacheKey _,
                        Func<Task<Either<ActionResult, TItem>>> entityProvider) => await entityProvider());

            return cacheService;
        }
    }
}

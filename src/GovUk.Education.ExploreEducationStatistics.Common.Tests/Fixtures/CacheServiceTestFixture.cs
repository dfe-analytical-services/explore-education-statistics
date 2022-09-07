#nullable enable
using System;
using GovUk.Education.ExploreEducationStatistics.Common.Cache;
using GovUk.Education.ExploreEducationStatistics.Common.Services.Interfaces;
using Moq;

namespace GovUk.Education.ExploreEducationStatistics.Common.Tests.Fixtures
{
    /// <summary>
    /// Class fixture for enabling/disabling caching and making available various Caching Services
    /// for tests needing to check caching annotations that use these services.
    /// </summary>
    public class CacheServiceTestFixture : IDisposable
    {
        protected const string CacheServiceTests = "Cache service tests";

        protected static readonly Mock<IBlobCacheService> BlobCacheService = new(MockBehavior.Strict);
        protected static readonly Mock<IBlobCacheService> PublicBlobCacheService = new(MockBehavior.Strict);
        protected static readonly Mock<IMemoryCacheService> MemoryCacheService = new(MockBehavior.Strict);

        protected CacheServiceTestFixture()
        {
            CacheAspect.Enabled = true;
            BlobCacheAttribute.AddService("default", BlobCacheService.Object);
            BlobCacheAttribute.AddService("public", PublicBlobCacheService.Object);
            MemoryCacheAttribute.AddService("default", MemoryCacheService.Object);
        }

        public void Dispose()
        {
            CacheAspect.Enabled = false;

            BlobCacheAttribute.ClearServices();
            BlobCacheService.Reset();
            PublicBlobCacheService.Reset();

            MemoryCacheAttribute.ClearServices();
            MemoryCacheService.Reset();
        }
    }
}

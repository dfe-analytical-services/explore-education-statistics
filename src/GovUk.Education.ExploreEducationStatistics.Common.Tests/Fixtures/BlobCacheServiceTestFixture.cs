#nullable enable
using System;
using GovUk.Education.ExploreEducationStatistics.Common.Cache;
using GovUk.Education.ExploreEducationStatistics.Common.Services.Interfaces;
using Moq;

namespace GovUk.Education.ExploreEducationStatistics.Common.Tests.Fixtures
{
    /// <summary>
    /// Class fixture for enabling/disabling caching and making available a <see cref="Mock<IBlobCacheService>"/>
    /// for tests needing to check caching annotations that use this service
    /// </summary>
    public class BlobCacheServiceTestFixture : IDisposable
    {
        protected const string BlobCacheServiceTests = "Blob cache service tests";

        protected static readonly Mock<IBlobCacheService> CacheService = new(MockBehavior.Strict);

        protected BlobCacheServiceTestFixture()
        {
            CacheAspect.Enabled = true;
            BlobCacheAttribute.AddService("default", CacheService.Object);
        }

        public void Dispose()
        {
            CacheAspect.Enabled = false;
            BlobCacheAttribute.ClearServices();
            CacheService.Reset();
        }
    }
}
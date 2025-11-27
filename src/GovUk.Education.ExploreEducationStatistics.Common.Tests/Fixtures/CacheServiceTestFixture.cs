#nullable enable
using GovUk.Education.ExploreEducationStatistics.Common.Cache;
using GovUk.Education.ExploreEducationStatistics.Common.Services.Interfaces;
using Moq;
using Xunit;

namespace GovUk.Education.ExploreEducationStatistics.Common.Tests.Fixtures;

/// <summary>
/// Class fixture for enabling/disabling caching and making available various Caching Services
/// for tests needing to check caching annotations that use these services.
/// </summary>
[Collection(CacheTestFixture.CollectionName)]
public class CacheServiceTestFixture : IDisposable
{
    public readonly Mock<IBlobCacheService> BlobCacheService = new(MockBehavior.Strict);
    public readonly Mock<IPublicBlobCacheService> PublicBlobCacheService = new(MockBehavior.Strict);
    public readonly Mock<IMemoryCacheService> MemoryCacheService = new(MockBehavior.Strict);

    // ReSharper disable once MemberCanBeProtected.Global
    public CacheServiceTestFixture()
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

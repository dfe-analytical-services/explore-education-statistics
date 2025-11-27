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
    public readonly Mock<IMemoryCacheService> MemoryCacheService = new(MockBehavior.Strict);

    // ReSharper disable once MemberCanBeProtected.Global
    public CacheServiceTestFixture()
    {
        CacheAspect.Enabled = true;
        MemoryCacheAttribute.AddService("default", MemoryCacheService.Object);
    }

    public void Dispose()
    {
        CacheAspect.Enabled = false;

        MemoryCacheAttribute.ClearServices();
        MemoryCacheService.Reset();
    }
}

#nullable enable
using GovUk.Education.ExploreEducationStatistics.Common.Cache;
using Xunit;

namespace GovUk.Education.ExploreEducationStatistics.Common.Tests.Fixtures;

/// <summary>
/// Class fixture for enabling/disabling caching and making available various Caching Services
/// for tests needing to check caching annotations that use these services.
/// </summary>
/// TODO EES-6450 - remove in follow-up PR.
[Collection(CacheTestFixture.CollectionName)]
public class CacheServiceTestFixture : IDisposable
{
    public void Dispose() { }
}

#nullable enable
using GovUk.Education.ExploreEducationStatistics.Common.Cache;

namespace GovUk.Education.ExploreEducationStatistics.Common.Tests.Fixtures;

/// <summary>
/// Simple class fixture for enabling/disabling
/// caching before and after a test suite.
/// </summary>
/// TODO EES-6450 - remove in follow-up PR.
public class CacheTestFixture : IDisposable
{
    public const string CollectionName = "Cache tests";

    public void Dispose() { }
}

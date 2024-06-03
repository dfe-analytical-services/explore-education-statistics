#nullable enable
using GovUk.Education.ExploreEducationStatistics.Common.Tests.Fixtures;
using Xunit;

namespace GovUk.Education.ExploreEducationStatistics.Common.Tests;

[Collection(CacheTestFixture.CollectionName)]
public abstract class IntegrationTest<TStartup>(
    TestApplicationFactory<TStartup> testApp
) : CacheServiceTestFixture,
    IClassFixture<TestApplicationFactory<TStartup>>,
    IClassFixture<CacheTestFixture>
    where TStartup : class
{
    protected readonly TestApplicationFactory<TStartup> TestApp = testApp;
}

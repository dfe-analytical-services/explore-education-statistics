using GovUk.Education.ExploreEducationStatistics.Common.Tests.Fixtures;
using Xunit;

namespace GovUk.Education.ExploreEducationStatistics.Common.Tests;

[Collection(CacheTestFixture.CollectionName)]
public abstract class IntegrationTest<TStartup>
    : CacheServiceTestFixture,
        IClassFixture<TestApplicationFactory<TStartup>>
    where TStartup : class
{
    protected readonly TestApplicationFactory<TStartup> TestApp;

    protected IntegrationTest(TestApplicationFactory<TStartup> testApp)
    {
        TestApp = testApp;
    }
}

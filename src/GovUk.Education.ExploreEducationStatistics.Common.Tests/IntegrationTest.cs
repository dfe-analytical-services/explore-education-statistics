using GovUk.Education.ExploreEducationStatistics.Common.Tests.Fixtures;
using Microsoft.AspNetCore.Mvc.Testing;
using Xunit;

namespace GovUk.Education.ExploreEducationStatistics.Common.Tests;

[Collection(CacheTestFixture.CollectionName)]
public abstract class IntegrationTest<TStartup> :
    CacheServiceTestFixture,
    IClassFixture<TestApplicationFactory<TStartup>>,
    IClassFixture<CacheTestFixture> where TStartup : class
{
    protected readonly WebApplicationFactory<TStartup> TestApp;

    protected IntegrationTest(TestApplicationFactory<TStartup> testApp)
    {
        TestApp = testApp;
    }
}
using GovUk.Education.ExploreEducationStatistics.Common.Tests.Fixtures;

namespace GovUk.Education.ExploreEducationStatistics.Public.Data.Api.Tests.Fixture;

public abstract class IntegrationTestFixture(TestApplicationFactory testApp) : IClassFixture<TestApplicationFactory>, IAsyncLifetime
{
    protected readonly DataFixture DataFixture = new();

    protected readonly TestApplicationFactory TestApp = testApp;

    public async Task InitializeAsync()
    {
        await TestApp.Initialize();
    }

    public async Task DisposeAsync()
    {
        await TestApp.ClearAllTestData();
    }
}

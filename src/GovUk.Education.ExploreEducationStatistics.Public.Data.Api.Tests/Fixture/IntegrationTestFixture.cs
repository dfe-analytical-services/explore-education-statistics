namespace GovUk.Education.ExploreEducationStatistics.Public.Data.Api.Tests.Fixture;

public abstract class IntegrationTestFixture : IClassFixture<TestApplicationFactory>, IAsyncLifetime
{
    protected readonly TestApplicationFactory TestApp;

    public IntegrationTestFixture(TestApplicationFactory testApp)
    {
        TestApp = testApp;
    }

    public async Task InitializeAsync()
    {
        await TestApp.Initialize();
    }

    public async Task DisposeAsync()
    {
        await TestApp.ClearAllTestData();
    }
}

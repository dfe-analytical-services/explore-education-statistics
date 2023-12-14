using GovUk.Education.ExploreEducationStatistics.Common.Tests.Fixtures;
using GovUk.Education.ExploreEducationStatistics.Public.Data.Api.Services.Interfaces;
using Moq;

namespace GovUk.Education.ExploreEducationStatistics.Public.Data.Api.Tests.Fixture;

public abstract class IntegrationTestFixture : IClassFixture<TestApplicationFactory>, IAsyncLifetime
{
    protected readonly DataFixture DataFixture = new();

    protected readonly TestApplicationFactory TestApp;

    protected HttpClient TestAppClient => TestApp.CreateClient();

    protected Mock<IContentApiClient> ContentApiClientMock => TestApp.ContentApiClientMock;

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
        await TestApp.Reset();
    }
}

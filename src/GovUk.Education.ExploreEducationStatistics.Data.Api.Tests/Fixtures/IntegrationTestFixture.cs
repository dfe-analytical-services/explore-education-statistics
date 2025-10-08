#nullable enable
using GovUk.Education.ExploreEducationStatistics.Common.Tests.Fixtures;
using GovUk.Education.ExploreEducationStatistics.Content.Model.Database;
using GovUk.Education.ExploreEducationStatistics.Data.Model.Database;
using Xunit;

namespace GovUk.Education.ExploreEducationStatistics.Data.Api.Tests.Fixtures;

[Collection(CacheTestFixture.CollectionName)]
public abstract class IntegrationTestFixture
    : CacheServiceTestFixture,
        IClassFixture<TestApplicationFactory>,
        IAsyncLifetime
{
    protected readonly DataFixture DataFixture = new();

    protected readonly TestApplicationFactory TestApp;

    internal IntegrationTestFixture(TestApplicationFactory testApp)
    {
        TestApp = testApp;
    }

    public async Task InitializeAsync()
    {
        await Task.CompletedTask;
    }

    public async Task DisposeAsync()
    {
        await TestApp.EnsureDatabaseDeleted<ContentDbContext>();
        await TestApp.EnsureDatabaseDeleted<StatisticsDbContext>();
        await TestApp.StopAzurite();
    }
}

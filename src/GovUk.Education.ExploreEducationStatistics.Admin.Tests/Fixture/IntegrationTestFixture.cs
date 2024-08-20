#nullable enable
using System.Threading.Tasks;
using GovUk.Education.ExploreEducationStatistics.Admin.Database;
using GovUk.Education.ExploreEducationStatistics.Common.Tests.Fixtures;
using GovUk.Education.ExploreEducationStatistics.Content.Model.Database;
using GovUk.Education.ExploreEducationStatistics.Data.Model.Database;
using GovUk.Education.ExploreEducationStatistics.Public.Data.Model.Database;
using Microsoft.Extensions.DependencyInjection;

namespace GovUk.Education.ExploreEducationStatistics.Admin.Tests.Fixture;

[Collection(CacheTestFixture.CollectionName)]
public abstract class IntegrationTestFixture(TestApplicationFactory testApp) :
    CacheServiceTestFixture,
    IClassFixture<TestApplicationFactory>,
    IClassFixture<CacheTestFixture>,
    IAsyncLifetime
{
    protected readonly DataFixture DataFixture = new();

    protected readonly TestApplicationFactory TestApp = testApp;

    public async Task InitializeAsync()
    {
        await TestApp.Initialize();
    }

    public async Task DisposeAsync()
    {
        await TestApp.EnsureDatabaseDeleted<ContentDbContext>();
        await TestApp.EnsureDatabaseDeleted<StatisticsDbContext>();
        await TestApp.EnsureDatabaseDeleted<UsersAndRolesDbContext>();
        await TestApp.ClearTestData<PublicDataDbContext>();
        TestApp.Services.GetRequiredService<PublicDataDbContext>().ChangeTracker.Clear();
    }
}

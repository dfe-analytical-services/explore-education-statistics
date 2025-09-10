using Azure.Data.Tables;
using GovUk.Education.ExploreEducationStatistics.Common.Extensions;
using GovUk.Education.ExploreEducationStatistics.Common.Options;
using GovUk.Education.ExploreEducationStatistics.Common.Services;
using GovUk.Education.ExploreEducationStatistics.Common.Tests.Extensions;
using GovUk.Education.ExploreEducationStatistics.Common.Tests.Fixtures;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;
using Xunit;

namespace GovUk.Education.ExploreEducationStatistics.Common.Tests;

public abstract class FunctionsIntegrationTest<TFunctionsIntegrationTestFixture>(
    FunctionsIntegrationTestFixture fixture) :
    IClassFixture<TFunctionsIntegrationTestFixture>
    where TFunctionsIntegrationTestFixture : FunctionsIntegrationTestFixture
{
    protected readonly DataFixture DataFixture = new();

    private readonly IHost _host = fixture
        .ConfigureTestHostBuilder()
        .Build();

    protected async Task AddTestData<TDbContext>(Action<TDbContext> supplier) where TDbContext : DbContext
    {
        await using var context = GetDbContext<TDbContext>();

        supplier.Invoke(context);
        await context.SaveChangesAsync();
    }

    protected async Task EnsureDatabaseDeleted<TDbContext>() where TDbContext : DbContext
    {
        await using var context = GetDbContext<TDbContext>();
        await context.Database.EnsureDeletedAsync();
    }

    protected TDbContext GetDbContext<TDbContext>() where TDbContext : DbContext
    {
        var scope = _host.Services.CreateScope();
        return scope.ServiceProvider.GetRequiredService<TDbContext>();
    }

    protected async Task ClearTestData<TDbContext>() where TDbContext : DbContext
    {
        var context = GetDbContext<TDbContext>();
        await context.ClearTestData();
    }

    // The following code was taken from this article which explains a fast way to clear all rows from Azure Table Storage
    // https://medium.com/medialesson/deleting-all-rows-from-azure-table-storage-as-fast-as-possible-79e03937c331
    protected async Task ClearAzureDataTableTestData(string connectionString)
    {
        var dataTableStorageService = new DataTableStorageService(connectionString);

        var tables = await dataTableStorageService.GetTables().ToListAsync();

        foreach (var table in tables)
        {
            var entities = await dataTableStorageService.QueryEntities<TableEntity>(
                tableName: table.Name,
                filter: null, // overload disambiguation
                select: new List<string>() { nameof(TableEntity.PartitionKey), nameof(TableEntity.RowKey) });

            await entities.AsPages().ForEachAwaitAsync(async page => {
                // Since we don't know how many rows the table has and the results are ordered by PartitonKey+RowKey
                // we'll delete each page immediately and not cache the whole table in memory
                await dataTableStorageService.BatchManipulateEntities(
                    tableName: table.Name, 
                    entities: page.Values, 
                    tableTransactionActionType: TableTransactionActionType.Delete);
            });
        }
    }

    protected void ResetDbContext<TDbContext>() where TDbContext : DbContext
    {
        var scope = _host.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<TDbContext>();

        db.Database.EnsureDeleted();
        db.Database.EnsureCreated();
    }

    protected TService GetRequiredService<TService>()
    {
        return _host.Services.GetRequiredService<TService>();
    }

    protected void SetDataSetVersionReplacementFeatureFlag(bool flag)
    {
        var options = GetRequiredService<IOptions<FeatureFlagsOptions>>().Value;
        options.EnableReplacementOfPublicApiDataSets = flag;
    }
}

// ReSharper disable once ClassNeverInstantiated.Global
public abstract class FunctionsIntegrationTestFixture
{
    public virtual IHostBuilder ConfigureTestHostBuilder()
    {
        return new HostBuilder()
            .ConfigureAppConfiguration((hostContext, config) =>
            {
                config
                    .AddJsonFile("local.settings.json", optional: true, reloadOnChange: false)
                    .AddEnvironmentVariables();
            })
            .ConfigureWebHostDefaults(builder => builder.UseIntegrationTestEnvironment())
            .ConfigureServices(services =>
                GetFunctionTypes().ForEach(functionType => services.AddScoped(functionType)));
    }

    protected virtual IEnumerable<Type> GetFunctionTypes()
    {
        return [];
    }
}

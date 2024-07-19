using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Azure;
using Azure.Data.Tables;
using GovUk.Education.ExploreEducationStatistics.Common.Extensions;
using GovUk.Education.ExploreEducationStatistics.Common.Tests.Extensions;
using GovUk.Education.ExploreEducationStatistics.Common.Tests.Fixtures;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
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
        var tableServiceClient = new TableServiceClient(connectionString);

        var tables = await tableServiceClient.QueryAsync().ToListAsync();

        foreach (var table in tables)
        {
            var tableClient = tableServiceClient.GetTableClient(table.Name);

            var entities = tableClient
                .QueryAsync<TableEntity>(select: new List<string>() { "PartitionKey", "RowKey" }, maxPerPage: 1000);

            await entities.AsPages().ForEachAwaitAsync(async page => {
                // Since we don't know how many rows the table has and the results are ordered by PartitonKey+RowKey
                // we'll delete each page immediately and not cache the whole table in memory
                await BatchManipulateEntities(tableClient, page.Values, TableTransactionActionType.Delete).ConfigureAwait(false);
            });
        }
    }

    /// <summary>
    /// Groups entities by PartitionKey into batches of max 100 for valid transactions
    /// </summary>
    /// <returns>List of Azure Responses for Transactions</returns>
    private static async Task<List<Response<IReadOnlyList<Response>>>> BatchManipulateEntities<T>(
        TableClient tableClient, 
        IEnumerable<T> entities, 
        TableTransactionActionType tableTransactionActionType) where T : class, ITableEntity, new()
    {
        var groups = entities.GroupBy(x => x.PartitionKey);
        var responses = new List<Response<IReadOnlyList<Response>>>();
        foreach (var group in groups)
        {
            List<TableTransactionAction> actions;
            var items = group.AsEnumerable();
            while (items.Any())
            {
                var batch = items.Take(100);
                items = items.Skip(100);

                actions = [.. batch.Select(e => new TableTransactionAction(tableTransactionActionType, e))];
                var response = await tableClient.SubmitTransactionAsync(actions).ConfigureAwait(false);
                responses.Add(response);
            }
        }
        return responses;
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
}

// ReSharper disable once ClassNeverInstantiated.Global
public abstract class FunctionsIntegrationTestFixture
{
    public virtual IHostBuilder ConfigureTestHostBuilder()
    {
        return new HostBuilder()
            .ConfigureAppConfiguration((hostContext, config) =>
            {
                hostContext.HostingEnvironment.EnvironmentName =
                    HostEnvironmentExtensions.IntegrationTestEnvironment;
                config.AddJsonFile("local.settings.json", optional: true, reloadOnChange: false);
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

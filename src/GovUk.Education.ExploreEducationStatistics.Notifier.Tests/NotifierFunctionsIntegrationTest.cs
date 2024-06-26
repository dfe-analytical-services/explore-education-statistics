using System.Threading.Tasks;
using GovUk.Education.ExploreEducationStatistics.Common.Tests;
using Microsoft.Azure.Cosmos.Table;
using Testcontainers.Azurite;
using Xunit;

namespace GovUk.Education.ExploreEducationStatistics.Notifier.Tests;

public abstract class NotifierFunctionsIntegrationTest
    (FunctionsIntegrationTestFixture fixture) : FunctionsIntegrationTest<NotifierFunctionsIntegrationTestFixture>(fixture);

// ReSharper disable once ClassNeverInstantiated.Global
public class NotifierFunctionsIntegrationTestFixture : FunctionsIntegrationTestFixture, IAsyncLifetime
{
    private readonly AzuriteContainer _azuriteContainer = new AzuriteBuilder()
        .WithImage("mcr.microsoft.com/azure-storage/azurite:3.27.0")
        .Build();

    public async Task DisposeAsync()
    {
        await _azuriteContainer.DisposeAsync();
    }

    public async Task InitializeAsync()
    {
        await _azuriteContainer.StartAsync();
    }

    public string TableStorageConnectionString()
    {
        return _azuriteContainer.GetConnectionString();
    }

    public async Task AddTestSubscription(string tableName, SubscriptionEntity subscription)
    {
        var storageAccount = CloudStorageAccount.Parse(TableStorageConnectionString());
        var tableClient = storageAccount.CreateCloudTableClient();
        var table = tableClient.GetTableReference(tableName);
        await table.CreateIfNotExistsAsync();

        await table.ExecuteAsync(TableOperation.InsertOrReplace(subscription));
    }
}

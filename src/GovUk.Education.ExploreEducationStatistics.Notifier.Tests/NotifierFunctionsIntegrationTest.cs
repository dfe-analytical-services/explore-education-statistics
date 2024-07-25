using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading;
using System.Threading.Tasks;
using Azure.Data.Tables;
using GovUk.Education.ExploreEducationStatistics.Common.Services;
using GovUk.Education.ExploreEducationStatistics.Common.Tests;
using GovUk.Education.ExploreEducationStatistics.Common.Tests.Extensions;
using GovUk.Education.ExploreEducationStatistics.Common.Tests.Utils;
using GovUk.Education.ExploreEducationStatistics.Notifier.Configuration;
using GovUk.Education.ExploreEducationStatistics.Notifier.Functions;
using GovUk.Education.ExploreEducationStatistics.Notifier.Model;
using Microsoft.Azure.Cosmos.Table;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;
using Moq;
using Notify.Interfaces;
using Testcontainers.Azurite;
using Xunit;

namespace GovUk.Education.ExploreEducationStatistics.Notifier.Tests;

public abstract class NotifierFunctionsIntegrationTest
    (NotifierFunctionsIntegrationTestFixture fixture) : FunctionsIntegrationTest<NotifierFunctionsIntegrationTestFixture>(fixture), IAsyncLifetime
{
    public Task InitializeAsync()
    {
        return Task.CompletedTask;
    }

    public Task DisposeAsync()
    {
        MockUtils.VerifyAllMocks(fixture._notificationClient);
        fixture._notificationClient.Reset();

        return ClearAzureDataTableTestData(StorageConnectionString());
    }

    protected string StorageConnectionString()
    {
        return fixture.StorageConnectionString();
    }

    public AppSettingsOptions GetAppSettingsOptions()
    {
        return GetRequiredService<IOptions<AppSettingsOptions>>().Value;
    }

    public GovUkNotifyOptions GetGovUkNotifyOptions()
    {
        return GetRequiredService<IOptions<GovUkNotifyOptions>>().Value;
    }

    public async Task AddTestSubscription(string tableName, SubscriptionEntity subscription)
    {
        var storageAccount = CloudStorageAccount.Parse(StorageConnectionString());
        var tableClient = storageAccount.CreateCloudTableClient();
        var table = tableClient.GetTableReference(tableName);
        await table.CreateIfNotExistsAsync();

        await table.ExecuteAsync(TableOperation.InsertOrReplace(subscription));
    }

    public async Task CreateApiSubscriptions(params ApiSubscription[] subscriptions)
    {
        var dataTableStorageService = new DataTableStorageService(StorageConnectionString());

        await dataTableStorageService.BatchManipulateEntities(
            tableName: NotifierTableStorageTableNames.ApiSubscriptionsTableName,
            entities: subscriptions,
            tableTransactionActionType: TableTransactionActionType.Add);
    }

    public async Task CreateApiSubscription(ApiSubscription subscription)
    {
        var dataTableStorageService = new DataTableStorageService(StorageConnectionString());

        await dataTableStorageService.CreateEntity(
            tableName: NotifierTableStorageTableNames.ApiSubscriptionsTableName,
            entity: subscription);
    }

    public async Task<ApiSubscription?> GetApiSubscriptionIfExists(
        Guid dataSetId,
        string email,
        IEnumerable<string>? select = null)
    {
        var dataTableStorageService = new DataTableStorageService(StorageConnectionString());

        return await dataTableStorageService.GetEntityIfExists<ApiSubscription>(
            tableName: NotifierTableStorageTableNames.ApiSubscriptionsTableName,
            partitionKey: dataSetId.ToString(),
            rowKey: email,
            select: select);
    }

    public async Task<IReadOnlyList<ApiSubscription>> QueryApiSubscriptions(
        Expression<Func<ApiSubscription, bool>>? filter = null,
        int? maxPerPage = null,
        IEnumerable<string>? select = null)
    {
        var dataTableStorageService = new DataTableStorageService(StorageConnectionString());

        var pagedSubscriptions = await dataTableStorageService.QueryEntities(
            tableName: NotifierTableStorageTableNames.ApiSubscriptionsTableName,
            filter: filter,
            maxPerPage: maxPerPage,
            select: select,
            cancellationToken: CancellationToken.None);

        var allSubscriptions = new List<ApiSubscription>();

        await pagedSubscriptions
            .AsPages()
            .ForEachAsync(page => allSubscriptions.AddRange(page.Values));

        return allSubscriptions;
    }
}

// ReSharper disable once ClassNeverInstantiated.Global
public class NotifierFunctionsIntegrationTestFixture : FunctionsIntegrationTestFixture, IAsyncLifetime
{
    public readonly Mock<INotificationClient> _notificationClient = new(MockBehavior.Strict);

    private readonly AzuriteContainer _azuriteContainer = new AzuriteBuilder()
        .WithImage("mcr.microsoft.com/azure-storage/azurite:3.31.0")
        .WithInMemoryPersistence()
        .Build();

    public async Task DisposeAsync()
    {
        await _azuriteContainer.DisposeAsync();
    }

    public async Task InitializeAsync()
    {
        await _azuriteContainer.StartAsync();
    }

    public string StorageConnectionString()
    {
        return _azuriteContainer.GetConnectionString();
    }

    public override IHostBuilder ConfigureTestHostBuilder()
    {
        return base
            .ConfigureTestHostBuilder()
            .ConfigureNotifierHostBuilder()
            .ConfigureAppConfiguration(builder =>
            {
                builder
                    .AddJsonFile("appsettings.IntegrationTest.json", optional: true, reloadOnChange: false)
                    .AddInMemoryCollection(new Dictionary<string, string?>
                    {
                        {
                            $"{AppSettingsOptions.Section}:{nameof(AppSettingsOptions.NotifierStorageConnectionString)}",
                            StorageConnectionString()
                        }
                    });
            })
            .ConfigureServices((hostContext, services) =>
            {
                services
                    .ReplaceService(_notificationClient);
            });
    }

    protected override IEnumerable<Type> GetFunctionTypes()
    {
        return
        [
            typeof(PublicationSubscriptionFunctions),
            typeof(ReleaseNotifier),
            typeof(ApiSubscriptionFunctions)
        ];
    }
}

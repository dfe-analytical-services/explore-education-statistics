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

        return ClearAzureDataTableTestData(TableStorageConnectionString());
    }

    public string TableStorageConnectionString()
    {
        return fixture.TableStorageConnectionString();
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
        var storageAccount = CloudStorageAccount.Parse(TableStorageConnectionString());
        var tableClient = storageAccount.CreateCloudTableClient();
        var table = tableClient.GetTableReference(tableName);
        await table.CreateIfNotExistsAsync();

        await table.ExecuteAsync(TableOperation.InsertOrReplace(subscription));
    }

    public async Task CreateApiSubscriptions(params ApiSubscription[] subscriptions)
    {
        var appSettingsOptions = GetAppSettingsOptions();

        var dataTableStorageService = new DataTableStorageService(TableStorageConnectionString());

        await dataTableStorageService.BatchManipulateEntities(
            tableName: appSettingsOptions.ApiSubscriptionsTableName,
            entities: subscriptions,
            tableTransactionActionType: TableTransactionActionType.Add);
    }

    public async Task CreateApiSubscription(ApiSubscription subscription)
    {
        var dataTableStorageService = new DataTableStorageService(TableStorageConnectionString());

        await dataTableStorageService.CreateEntity(
            tableName: Constants.NotifierTableStorageTableNames.ApiSubscriptionsTableName,
            entity: subscription);
    }

    public async Task<ApiSubscription?> GetApiSubscriptionIfExists(
        Guid dataSetId,
        string email,
        IEnumerable<string>? select = null)
    {
        var dataTableStorageService = new DataTableStorageService(TableStorageConnectionString());

        return await dataTableStorageService.GetEntityIfExists<ApiSubscription>(
            tableName: Constants.NotifierTableStorageTableNames.ApiSubscriptionsTableName,
            partitionKey: dataSetId.ToString(),
            rowKey: email,
            select: select);
    }

    public async Task<IReadOnlyList<ApiSubscription>> QueryApiSubscriptions(
        Expression<Func<ApiSubscription, bool>>? filter = null,
        int? maxPerPage = null,
        IEnumerable<string>? select = null)
    {
        var appSettingsOptions = GetRequiredService<IOptions<AppSettingsOptions>>();

        var dataTableStorageService = new DataTableStorageService(appSettingsOptions.Value.TableStorageConnectionString);

        var pagedSubscriptions = await dataTableStorageService.QueryEntitiesAsync(
            tableName: appSettingsOptions.Value.ApiSubscriptionsTableName,
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

    public string TableStorageConnectionString()
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
                    .AddInMemoryCollection(new List<KeyValuePair<string, string?>>
                    {
                        new($"{nameof(AppSettingsOptions.AppSettings)}:{nameof(AppSettingsOptions.TableStorageConnectionString)}", TableStorageConnectionString())
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

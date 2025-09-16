using System.Linq.Expressions;
using Azure.Data.Tables;
using GovUk.Education.ExploreEducationStatistics.Common.Services;
using GovUk.Education.ExploreEducationStatistics.Common.Tests;
using GovUk.Education.ExploreEducationStatistics.Common.Tests.Extensions;
using GovUk.Education.ExploreEducationStatistics.Common.Tests.Utils;
using GovUk.Education.ExploreEducationStatistics.Notifier.Functions;
using GovUk.Education.ExploreEducationStatistics.Notifier.Model;
using GovUk.Education.ExploreEducationStatistics.Notifier.Options;
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
        MockUtils.VerifyAllMocks(fixture.NotificationClient);
        fixture.NotificationClient.Reset();

        return ClearAzureDataTableTestData(StorageConnectionString());
    }

    protected string StorageConnectionString()
    {
        return fixture.StorageConnectionString();
    }

    protected AppOptions GetAppOptions()
    {
        return GetRequiredService<IOptions<AppOptions>>().Value;
    }

    protected GovUkNotifyOptions GetGovUkNotifyOptions()
    {
        return GetRequiredService<IOptions<GovUkNotifyOptions>>().Value;
    }

    protected async Task CreateApiSubscriptions(params ApiSubscription[] subscriptions)
    {
        var dataTableStorageService = new DataTableStorageService(StorageConnectionString());

        await dataTableStorageService.BatchManipulateEntities(
            tableName: NotifierTableStorage.ApiSubscriptionsTable,
            entities: subscriptions,
            tableTransactionActionType: TableTransactionActionType.Add);
    }

    protected async Task CreateApiSubscription(ApiSubscription subscription)
    {
        var dataTableStorageService = new DataTableStorageService(StorageConnectionString());

        await dataTableStorageService.CreateEntity(
            tableName: NotifierTableStorage.ApiSubscriptionsTable,
            entity: subscription);
    }

    protected async Task<ApiSubscription?> GetApiSubscriptionIfExists(
        Guid dataSetId,
        string email,
        IEnumerable<string>? select = null)
    {
        var dataTableStorageService = new DataTableStorageService(StorageConnectionString());

        return await dataTableStorageService.GetEntityIfExists<ApiSubscription>(
            tableName: NotifierTableStorage.ApiSubscriptionsTable,
            partitionKey: dataSetId.ToString(),
            rowKey: email,
            select: select);
    }

    protected async Task<IReadOnlyList<ApiSubscription>> QueryApiSubscriptions(
        Expression<Func<ApiSubscription, bool>>? filter = null,
        int? maxPerPage = null,
        IEnumerable<string>? select = null)
    {
        var dataTableStorageService = new DataTableStorageService(StorageConnectionString());

        var pagedSubscriptions = await dataTableStorageService.QueryEntities(
            tableName: NotifierTableStorage.ApiSubscriptionsTable,
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
    public readonly Mock<INotificationClient> NotificationClient = new(MockBehavior.Strict);

    private readonly AzuriteContainer _azuriteContainer = new AzuriteBuilder()
        .WithImage("mcr.microsoft.com/azure-storage/azurite:3.35.0")
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
                            $"{AppOptions.Section}:{nameof(AppOptions.NotifierStorageConnectionString)}",
                            StorageConnectionString()
                        }
                    });
            })
            .ConfigureServices((hostContext, services) =>
            {
                services
                    .ReplaceService(NotificationClient);
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

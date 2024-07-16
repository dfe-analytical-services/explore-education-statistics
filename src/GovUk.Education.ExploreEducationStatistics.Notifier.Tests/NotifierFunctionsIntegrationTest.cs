using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using GovUk.Education.ExploreEducationStatistics.Common.Services;
using GovUk.Education.ExploreEducationStatistics.Common.Tests;
using GovUk.Education.ExploreEducationStatistics.Common.Tests.Extensions;
using GovUk.Education.ExploreEducationStatistics.Common.Tests.Utils;
using GovUk.Education.ExploreEducationStatistics.Notifier.Configuration;
using GovUk.Education.ExploreEducationStatistics.Notifier.Functions;
using GovUk.Education.ExploreEducationStatistics.Notifier.Model;
using GovUk.Education.ExploreEducationStatistics.Notifier.Services.Interfaces;
using Microsoft.Azure.Cosmos.Table;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;
using Moq;
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
        MockUtils.VerifyAllMocks(fixture._emailService);
        fixture._emailService.Reset();

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

    public async Task CreateApiSubscription(ApiSubscription subscription)
    {
        var appSettingsOptions = GetAppSettingsOptions();

        var dataTableStorageService = new DataTableStorageService(TableStorageConnectionString());

        await dataTableStorageService.CreateEntityAsync(
            tableName: appSettingsOptions.ApiSubscriptionsTableName,
            entity: subscription);
    }

    public async Task<ApiSubscription?> GetApiSubscriptionIfExists(
        Guid dataSetId,
        string email,
        IEnumerable<string>? select = null)
    {
        var appSettingsOptions = GetRequiredService<IOptions<AppSettingsOptions>>();

        var dataTableStorageService = new DataTableStorageService(appSettingsOptions.Value.TableStorageConnectionString);

        return await dataTableStorageService.GetEntityIfExistsAsync<ApiSubscription>(
            tableName: appSettingsOptions.Value.ApiSubscriptionsTableName,
            partitionKey: email,
            rowKey: dataSetId.ToString(),
            select: select);
    }
}

// ReSharper disable once ClassNeverInstantiated.Global
public class NotifierFunctionsIntegrationTestFixture : FunctionsIntegrationTestFixture, IAsyncLifetime
{
    public readonly Mock<IEmailService> _emailService = new(MockBehavior.Strict);

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
                    .ReplaceService(_emailService);
            });
    }

    protected override IEnumerable<Type> GetFunctionTypes()
    {
        return
        [
            typeof(PublicationSubscriptionManager),
            typeof(ReleaseNotifier),
            typeof(ApiSubscriptionManager)
        ];
    }
}

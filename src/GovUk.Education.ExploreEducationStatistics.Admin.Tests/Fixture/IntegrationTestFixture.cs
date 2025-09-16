#nullable enable
using DotNet.Testcontainers.Containers;
using GovUk.Education.ExploreEducationStatistics.Admin.Database;
using GovUk.Education.ExploreEducationStatistics.Common.Services;
using GovUk.Education.ExploreEducationStatistics.Common.Services.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Common.Tests.Extensions;
using GovUk.Education.ExploreEducationStatistics.Common.Tests.Fixtures;
using GovUk.Education.ExploreEducationStatistics.Content.Model.Database;
using GovUk.Education.ExploreEducationStatistics.Data.Model.Database;
using GovUk.Education.ExploreEducationStatistics.Data.Processor.Model;
using GovUk.Education.ExploreEducationStatistics.Public.Data.Model.Database;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Testcontainers.Azurite;

namespace GovUk.Education.ExploreEducationStatistics.Admin.Tests.Fixture;

[Collection(CacheTestFixture.CollectionName)]
public abstract class IntegrationTestFixture(TestApplicationFactory testApp) :
    CacheServiceTestFixture,
    IClassFixture<TestApplicationFactory>,
    IClassFixture<CacheTestFixture>,
    IAsyncLifetime
{
    private readonly AzuriteContainer _azuriteContainer = new AzuriteBuilder()
        .WithImage("mcr.microsoft.com/azure-storage/azurite:3.35.0")
        .Build();

    protected readonly DataFixture DataFixture = new();

    protected readonly TestApplicationFactory TestApp = testApp;

    /// <summary>
    /// Start the Azurite container. Once started, the test app must also
    /// be configured with <see cref="WithAzurite"/> to use it.
    /// </summary>
    /// <remarks>
    /// We don't start the Azurite container in a class fixture as there currently
    /// isn't a good way to clear it after each test. The current approach is to
    /// restart the container for each test case (which is quite slow).
    /// See: https://github.com/Azure/Azurite/issues/588.
    /// For now, we should manually control the Azurite container's lifecycle by
    /// calling this on a case-by-case basis.
    /// </remarks>
    public async Task InitializeWithAzurite()
    {
        await TestApp.Initialize();
        await _azuriteContainer.StartAsync();
    }

    public virtual async Task InitializeAsync()
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
        await _azuriteContainer.DisposeAsync();
    }

    public WebApplicationFactory<TestStartup> WithAzurite(
        WebApplicationFactory<TestStartup>? testApp = null,
        bool enabled = true)
    {
        testApp ??= TestApp;

        if (!enabled)
        {
            return testApp;
        }

        if (_azuriteContainer.State != TestcontainersStates.Running)
        {
            throw new InvalidOperationException(
                $"Azurite container must be started via '{nameof(InitializeWithAzurite)}' method first");
        }

        return testApp.WithWebHostBuilder(builder =>
        {
            builder
                .ConfigureAppConfiguration((_, config) =>
                {
                    config.AddInMemoryCollection(
                    [
                        new KeyValuePair<string, string?>("PublicStorage", _azuriteContainer.GetConnectionString()),
                        new KeyValuePair<string, string?>("PublisherStorage", _azuriteContainer.GetConnectionString()),
                    ]);
                })
                .ConfigureServices(services =>
                {
                    services.ReplaceService<IPublicBlobStorageService>(sp =>
                        new PublicBlobStorageService(
                            connectionString: _azuriteContainer.GetConnectionString(),
                            logger: sp.GetRequiredService<ILogger<IBlobStorageService>>(),
                            sasService: sp.GetRequiredService<IBlobSasService>()
                        )
                    );
                    services.ReplaceService<IPrivateBlobStorageService>(sp =>
                        new PrivateBlobStorageService(
                            connectionString: _azuriteContainer.GetConnectionString(),
                            logger: sp.GetRequiredService<ILogger<IBlobStorageService>>(),
                            sasService: sp.GetRequiredService<IBlobSasService>()
                        )
                    );
                    services.ReplaceService<IPublisherTableStorageService>(_ =>
                        new PublisherTableStorageService(_azuriteContainer.GetConnectionString())
                    );
                    services.ReplaceService<IDataProcessorClient>(_ =>
                        new DataProcessorClient(_azuriteContainer.GetConnectionString())
                    );
                    services.AddTransient<IPublicBlobCacheService, PublicBlobCacheService>();
                    services.AddTransient<IPrivateBlobCacheService, PrivateBlobCacheService>();
                });
        });
    }
}

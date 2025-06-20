#nullable enable
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using DotNet.Testcontainers.Containers;
using GovUk.Education.ExploreEducationStatistics.Common.Services.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Common.Tests.Fixtures;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.Configuration;
using Testcontainers.Azurite;
using Xunit;
using GovUk.Education.ExploreEducationStatistics.Common.Services;
using GovUk.Education.ExploreEducationStatistics.Common.Tests.Extensions;
using GovUk.Education.ExploreEducationStatistics.Content.Services.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Content.Services.Tests.Services;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace GovUk.Education.ExploreEducationStatistics.Content.Api.Tests.Fixtures;

[Collection(CacheTestFixture.CollectionName)]
public abstract class IntegrationTestFixture(TestApplicationFactory testApp) :
    CacheServiceTestFixture,
    IClassFixture<TestApplicationFactory>,
    IAsyncLifetime
{
    private readonly AzuriteContainer _azuriteContainer = new AzuriteBuilder()
        .WithImage("mcr.microsoft.com/azure-storage/azurite:3.34.0")
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
    public async Task StartAzurite()
    {
        await _azuriteContainer.StartAsync();
    }

    public virtual async Task InitializeAsync()
    {
        await Task.CompletedTask;
    }

    public virtual async Task DisposeAsync()
    {
        await TestApp.ClearAllTestData();
        await _azuriteContainer.DisposeAsync();
    }

    public WebApplicationFactory<Startup> BuildWebApplicationFactory(
        List<Action<IWebHostBuilder>> webHostBuilderConfigFuncs)
    {
        return TestApp.WithWebHostBuilder(builder =>
        {
            foreach (var configFunc in webHostBuilderConfigFuncs)
            {
                configFunc(builder);
            }
        });
    }

    public Action<IWebHostBuilder> WithAnalytics()
    {
        TestApp.AddAppSettings("appsettings.IntegrationTest.AnalyticsEnabled.json");

        return builder =>
        {
            builder.ConfigureServices(services =>
            {
                services.ReplaceService<IAnalyticsPathResolver>(_ =>
                    new TestAnalyticsPathResolver(), optional: true);
            });
        };
    }

    public Action<IWebHostBuilder> WithAzurite()
    {
        if (_azuriteContainer.State != TestcontainersStates.Running)
        {
            throw new InvalidOperationException(
                $"Azurite container must be started via '{nameof(StartAzurite)}' method first");
        }

        return builder =>
        {
            builder
                .ConfigureAppConfiguration((_, config) =>
                {
                    config.AddInMemoryCollection(
                    [
                        new KeyValuePair<string, string?>("PublicStorage", _azuriteContainer.GetConnectionString())
                    ]);
                })
                .ConfigureServices(services =>
                {
                    services.ReplaceService<IPublicBlobStorageService>(sp =>
                        new PublicBlobStorageService(
                            _azuriteContainer.GetConnectionString(),
                            sp.GetRequiredService<ILogger<IBlobStorageService>>()
                        )
                    );
                });
        };
    }
}

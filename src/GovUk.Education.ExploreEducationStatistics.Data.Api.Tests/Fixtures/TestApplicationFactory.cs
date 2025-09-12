#nullable enable
using DotNet.Testcontainers.Containers;
using GovUk.Education.ExploreEducationStatistics.Common.Services;
using GovUk.Education.ExploreEducationStatistics.Common.Services.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Common.Tests.Extensions;
using GovUk.Education.ExploreEducationStatistics.Common.Tests.Fixtures;
using GovUk.Education.ExploreEducationStatistics.Content.Model.Database;
using GovUk.Education.ExploreEducationStatistics.Data.Model.Database;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Moq;
using Testcontainers.Azurite;

namespace GovUk.Education.ExploreEducationStatistics.Data.Api.Tests.Fixtures;

public sealed class TestApplicationFactory : TestApplicationFactory<Startup>
{
    private readonly AzuriteContainer _azuriteContainer = new AzuriteBuilder()
        .WithImage("mcr.microsoft.com/azure-storage/azurite:3.35.0")
        .WithInMemoryPersistence()
        .Build();

    public override async ValueTask DisposeAsync()
    {
        await _azuriteContainer.DisposeAsync();
    }

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

    public async Task StopAzurite()
    {
        await _azuriteContainer.StopAsync();
    }

    public WebApplicationFactory<Startup> WithAzurite(bool enabled = true)
    {
        if (!enabled)
        {
            return this;
        }

        if (_azuriteContainer.State != TestcontainersStates.Running)
        {
            throw new InvalidOperationException(
                $"Azurite container must be started via '{nameof(StartAzurite)}' method first");
        }

        return WithWebHostBuilder(builder =>
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
                            connectionString: _azuriteContainer.GetConnectionString(),
                            logger: sp.GetRequiredService<ILogger<IBlobStorageService>>(),
                            sasService: sp.GetRequiredService<IBlobSasService>()
                        )
                    );
                });
        });
    }

    protected override IHostBuilder CreateHostBuilder()
    {
        return base
            .CreateHostBuilder()
            .ConfigureServices(services =>
            {
                services
                    .UseInMemoryDbContext<ContentDbContext>()
                    .UseInMemoryDbContext<StatisticsDbContext>()
                    .ReplaceService(new Mock<IPublicBlobStorageService>());
            });
    }
}

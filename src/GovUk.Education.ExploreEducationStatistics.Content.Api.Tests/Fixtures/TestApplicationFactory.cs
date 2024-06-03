#nullable enable
using System.Collections.Generic;
using System.Threading.Tasks;
using GovUk.Education.ExploreEducationStatistics.Common.Services;
using GovUk.Education.ExploreEducationStatistics.Common.Services.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Common.Tests.Extensions;
using GovUk.Education.ExploreEducationStatistics.Common.Tests.Fixtures;
using GovUk.Education.ExploreEducationStatistics.Content.Model.Database;
using GovUk.Education.ExploreEducationStatistics.Data.Model.Database;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Configuration.Memory;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Testcontainers.Azurite;

namespace GovUk.Education.ExploreEducationStatistics.Content.Api.Tests.Fixtures;

public class TestApplicationFactory : TestApplicationFactory<Startup>
{
    private readonly AzuriteContainer _azuriteContainer = new AzuriteBuilder()
        .WithImage("mcr.microsoft.com/azure-storage/azurite:3.27.0")
        .Build();

    public async Task Initialize()
    {
        await _azuriteContainer.StartAsync();
    }

    public override async ValueTask DisposeAsync()
    {
        await _azuriteContainer.DisposeAsync();
    }

    protected override IHostBuilder CreateHostBuilder()
    {
        return base
            .CreateHostBuilder()
            .ConfigureAppConfiguration(config =>
            {
                config.Add(new MemoryConfigurationSource
                {
                    InitialData =
                    [
                        new KeyValuePair<string, string?>(
                            "PublicStorage",
                            _azuriteContainer.GetConnectionString()
                        )
                    ]
                });
            })
            .ConfigureServices(services =>
            {
                services
                    .UseInMemoryDbContext<ContentDbContext>()
                    .UseInMemoryDbContext<StatisticsDbContext>();
            });
    }
}

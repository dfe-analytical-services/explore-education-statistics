#nullable enable
using GovUk.Education.ExploreEducationStatistics.Common.Extensions;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Console;
using static GovUk.Education.ExploreEducationStatistics.Common.Extensions.HostEnvironmentExtensions;

namespace GovUk.Education.ExploreEducationStatistics.Common.Tests.Fixtures;

/// <summary>
/// Factory for creating test applications in integration tests.
/// </summary>
/// <see cref="https://docs.microsoft.com/en-us/aspnet/core/test/integration-tests"/>
// ReSharper disable once ClassNeverInstantiated.Global
public class TestApplicationFactory<TStartup> : WebApplicationFactory<TStartup> where TStartup : class
{
    protected override IHostBuilder CreateHostBuilder()
    {
        return Host
            .CreateDefaultBuilder()
            .ConfigureLogging(
                builder =>
                {
                    builder
                        .AddFilter<ConsoleLoggerProvider>("Default", LogLevel.Warning)
                        .AddFilter<ConsoleLoggerProvider>("Microsoft", LogLevel.Warning);
                }
            )
            .ConfigureWebHostDefaults(builder =>
            {
                builder
                    .UseStartup<TStartup>()
                    .UseIntegrationTestEnvironment()
                    .UseTestServer();
            })
            .ConfigureAppConfiguration(config =>
            {
                config.AddConfiguration(new ConfigurationBuilder()
                    .AddJsonFile($"appsettings.{IntegrationTestEnvironment}.json", optional: true)
                    .Build());
            });
    }
}
#nullable enable
using GovUk.Education.ExploreEducationStatistics.Common.Tests.Extensions;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.EntityFrameworkCore;
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
public class TestApplicationFactory<TStartup> : WebApplicationFactory<TStartup>
    where TStartup : class
{
    public async Task AddTestData<TDbContext>(Action<TDbContext> supplier)
        where TDbContext : DbContext
    {
        await using var context = GetDbContext<TDbContext>();

        supplier.Invoke(context);
        await context.SaveChangesAsync();
    }

    public async Task EnsureDatabaseDeleted<TDbContext>()
        where TDbContext : DbContext
    {
        await using var context = this.GetDbContext<TDbContext, TStartup>();
        await context.Database.EnsureDeletedAsync();
    }

    public TDbContext GetDbContext<TDbContext>()
        where TDbContext : DbContext
    {
        return this.GetDbContext<TDbContext, TStartup>();
    }

    protected override IHostBuilder CreateHostBuilder()
    {
        return Host.CreateDefaultBuilder()
            .ConfigureLogging(builder =>
            {
                builder
                    .AddFilter<ConsoleLoggerProvider>("Default", LogLevel.Warning)
                    .AddFilter<ConsoleLoggerProvider>("Microsoft", LogLevel.Warning);

                // Uncomment to add SQL logging to the debug console.
                // .AddFilter<ConsoleLoggerProvider>((category, level) =>
                //     category == DbLoggerCategory.Database.Command.Name
                //     && level == LogLevel.Information);
            })
            .ConfigureWebHostDefaults(builder =>
            {
                builder.UseStartup<TStartup>().UseIntegrationTestEnvironment().UseTestServer();
            })
            .ConfigureAppConfiguration(config =>
            {
                config.AddConfiguration(
                    new ConfigurationBuilder()
                        .AddJsonFile(
                            $"appsettings.{IntegrationTestEnvironment}.json",
                            optional: true
                        )
                        .Build()
                );
            });
    }
}

#nullable enable
using System;
using System.Net.Http;
using System.Threading.Tasks;
using GovUk.Education.ExploreEducationStatistics.Common.Extensions;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Console;
using static GovUk.Education.ExploreEducationStatistics.Common.Extensions.HostEnvironmentExtensions;

namespace GovUk.Education.ExploreEducationStatistics.Common.Tests.Fixtures;

/// <summary>
/// Factory for creating test applications in integration tests.
/// </summary>
/// <see href="https://docs.microsoft.com/en-us/aspnet/core/test/integration-tests"/>
public class TestApplicationFactory<TStartup, TWebApp> :
    IDisposable,
    IAsyncDisposable
    where TStartup : class
    where TWebApp : WebApplicationFactory<TStartup>, new()
{
    internal WebApplicationFactory<TStartup> App = new();

    public virtual void Dispose()
    {
        App.Dispose();
        GC.SuppressFinalize(this);
    }

    public virtual async ValueTask DisposeAsync()
    {
        await App.DisposeAsync();
        GC.SuppressFinalize(this);
    }

    public virtual TestApplicationFactory<TStartup, TWebApp> WithWebHostBuilder(Action<IWebHostBuilder> configure)
    {
        return new TestApplicationFactory<TStartup, TWebApp>
        {
            App = App.WithWebHostBuilder(configure)
        };
    }

    public virtual TestApplicationFactory<TStartup, TWebApp> ConfigureServices(Action<IServiceCollection> configureServices)
    {
        return new TestApplicationFactory<TStartup, TWebApp>
        {
            App = App.WithWebHostBuilder(builder => builder.ConfigureServices(configureServices))
        };
    }

    public HttpClient CreateClient()
        => App.CreateClient();

    public HttpClient CreateClient(WebApplicationFactoryClientOptions options)
        => App.CreateClient(options);

    public IServiceProvider Services => App.Services;

    public async Task AddTestData<TDbContext>(Action<TDbContext> supplier) where TDbContext : DbContext
    {
        await using var context = GetDbContext<TDbContext>();

        supplier.Invoke(context);
        await context.SaveChangesAsync();
    }

    public async Task EnsureDatabaseDeleted<TDbContext>() where TDbContext : DbContext
    {
        await using var context = GetDbContext<TDbContext>();
        await context.Database.EnsureDeletedAsync();
    }

    public TDbContext GetDbContext<TDbContext>() where TDbContext : DbContext
    {
        var scope = Services.CreateScope();
        return scope.ServiceProvider.GetRequiredService<TDbContext>();
    }

    public class WebApp : WebApplicationFactory<TStartup>
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
}

public class TestApplicationFactory<TStartup, TWebApp, TFactory> :
    TestApplicationFactory<TStartup, TWebApp>
    where TStartup : class
    where TWebApp : WebApplicationFactory<TStartup>, new()
    where TFactory : TestApplicationFactory<TStartup, TWebApp>, new()
{
    public override TFactory WithWebHostBuilder(Action<IWebHostBuilder> configure)
    {
        return new TFactory
        {
            App = this.App.WithWebHostBuilder(configure)
        };
    }
    
    public override TFactory ConfigureServices(Action<IServiceCollection> configureServices)
    {
        return new TFactory
        {
            App = this.App.WithWebHostBuilder(builder => builder.ConfigureServices(configureServices))
        };
    }
}

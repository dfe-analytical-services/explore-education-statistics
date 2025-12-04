using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Console;

namespace GovUk.Education.ExploreEducationStatistics.Common.IntegrationTests.WebApp;

/// <summary>
///
/// A builder for a WebApplicationFactory based on a minimal API project structure.
///
/// This builder constructs a new MinimalApiWebApplicationFactory based upon <see cref="TStartup"/>.
///
/// The MinimalApiWebApplicationFactory subclass controls the reconfiguring of the WebApplicationFactory that
/// is generated.
///
/// The MinimalApiWebApplicationFactory firstly creates a new standard HostBuilder.
/// The MinimalApiWebApplicationFactory then creates a new web host builder, tells it that we are operating
/// in the "IntegrationTests" environment and registers the standard TestServer support, which creates a testable
/// web host and lifetime.
/// The MinimalApiWebApplicationFactory then applies any specific changes to registered services and configuration
/// that specific tests need.
/// The MinimalApiWebApplicationFactory also ensures that an "appsettings.IntegrationTests.json" settings file is ready
/// if it exists.
/// The MinimalApiWebApplicationFactory finally allows us to set the log levels used during test execution.
///
/// </summary>
public class OptimisedWebApplicationFactoryMinimalApiBuilder<TStartup>
    : OptimisedWebApplicationFactoryBuilderBase<TStartup>
    where TStartup : class
{
    public override WebApplicationFactory<TStartup> Build(
        List<Action<IServiceCollection>> serviceModifications,
        List<Action<IConfigurationBuilder>> configModifications
    )
    {
        // Return a custom WebApplicationFactory subclass that applies any requested changes to service
        // registrations and configuration changes.
        return new MinimalApiWebApplicationFactory(serviceModifications, configModifications);
    }

    /// <summary>
    /// Custom WebApplicationFactory that overrides CreateHostBuilder() to allow us to reconfigure it.
    /// </summary>
    private sealed class MinimalApiWebApplicationFactory(
        List<Action<IServiceCollection>> serviceModifications,
        List<Action<IConfigurationBuilder>> configModifications
    ) : WebApplicationFactory<TStartup>
    {
        protected override IHostBuilder CreateHostBuilder()
        {
            return Host.CreateDefaultBuilder()
                .ConfigureWebHostDefaults(builder =>
                {
                    builder.UseStartup<TStartup>().UseIntegrationTestEnvironment().UseTestServer();
                })
                .ConfigureAppConfiguration(config =>
                {
                    foreach (var modification in configModifications)
                    {
                        modification(config);
                    }
                })
                .ConfigureServices(services =>
                {
                    foreach (var modification in serviceModifications)
                    {
                        modification(services);
                    }
                })
                .ConfigureAppConfiguration(config =>
                {
                    config.AddConfiguration(
                        new ConfigurationBuilder()
                            .AddJsonFile(
                                $"appsettings.{HostEnvironmentExtensions.IntegrationTestEnvironment}.json",
                                optional: true
                            )
                            .Build()
                    );
                })
                .ConfigureLogging(builder =>
                {
                    builder
                        .AddFilter<ConsoleLoggerProvider>("Default", LogLevel.Warning)
                        .AddFilter<ConsoleLoggerProvider>("Microsoft", LogLevel.Warning);

                    // Uncomment to add SQL logging to the debug console.
                    // .AddFilter<ConsoleLoggerProvider>((category, level) =>
                    //     category == DbLoggerCategory.Database.Command.Name
                    //     && level == LogLevel.Information);
                });
        }
    }
}

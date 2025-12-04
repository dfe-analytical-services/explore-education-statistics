using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace GovUk.Education.ExploreEducationStatistics.Common.IntegrationTests.WebApp;

/// <summary>
///
/// A builder for a WebApplicationFactory based on a classic MVC project structure.
///
/// This builder firstly constructs a new WebApplicationFactory based upon <see cref="TStartup"/> and then generates
/// a new host builder based on its configuration, so that we can configure it further.
/// The builder then tells the host that we are operating in the "IntegrationTests" environment.
/// The builder then registers the standard TestServer support, which creates a testable host and lifetime.
/// The builder then applies any specific changes to registered services and configuration that specific tests need.
/// The builder also ensures that an "appsettings.IntegrationTests.json" settings file is ready if it exists.
///
/// </summary>
public class OptimisedWebApplicationFactoryMvcBuilder<TStartup> : OptimisedWebApplicationFactoryBuilderBase<TStartup>
    where TStartup : class
{
    public override WebApplicationFactory<TStartup> Build(
        List<Action<IServiceCollection>> serviceModifications,
        List<Action<IConfigurationBuilder>> configModifications
    )
    {
        return new WebApplicationFactory<TStartup>().WithWebHostBuilder(builder =>
        {
            builder
                .UseIntegrationTestEnvironment()
                .UseTestServer()
                .ConfigureServices(services =>
                {
                    serviceModifications.ForEach(registrations => registrations.Invoke(services));
                })
                .ConfigureAppConfiguration(
                    (_, config) =>
                    {
                        configModifications.ForEach(registrations => registrations.Invoke(config));
                    }
                )
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
                });
        });
    }
}

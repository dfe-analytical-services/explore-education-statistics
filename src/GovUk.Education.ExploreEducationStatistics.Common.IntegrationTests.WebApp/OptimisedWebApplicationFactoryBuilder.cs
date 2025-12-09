using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace GovUk.Education.ExploreEducationStatistics.Common.IntegrationTests.WebApp;

public class OptimisedWebApplicationFactoryBuilder<TStartup>
    where TStartup : class
{
    private readonly List<Action<IServiceCollection>> _serviceModifications = [];
    private readonly List<Action<IConfigurationBuilder>> _configModifications = [];

    public WebApplicationFactory<TStartup> Build()
    {
        return new WebApplicationFactory<TStartup>().WithWebHostBuilder(builder =>
        {
            builder
                .UseIntegrationTestEnvironment()
                .UseTestServer()
                .ConfigureServices(services =>
                {
                    _serviceModifications.ForEach(registrations => registrations.Invoke(services));
                })
                .ConfigureAppConfiguration(
                    (_, config) =>
                    {
                        _configModifications.ForEach(registrations => registrations.Invoke(config));
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

    public OptimisedWebApplicationFactoryBuilder<TStartup> AddServiceModifications(
        Action<IServiceCollection> serviceRegistration
    )
    {
        _serviceModifications.Add(serviceRegistration);
        return this;
    }

    public OptimisedWebApplicationFactoryBuilder<TStartup> AddConfigModifications(
        Action<IConfigurationBuilder> configRegistration
    )
    {
        _configModifications.Add(configRegistration);
        return this;
    }
}

internal static class HostEnvironmentExtensions
{
    public const string IntegrationTestEnvironment = "IntegrationTest";

    public static IWebHostBuilder UseIntegrationTestEnvironment(this IWebHostBuilder hostBuilder)
    {
        return hostBuilder.UseEnvironment(IntegrationTestEnvironment);
    }
}

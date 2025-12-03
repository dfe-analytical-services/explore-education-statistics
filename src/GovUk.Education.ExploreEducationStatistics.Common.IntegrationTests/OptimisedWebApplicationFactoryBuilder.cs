using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace GovUk.Education.ExploreEducationStatistics.Common.IntegrationTests;

public class OptimisedWebApplicationFactoryBuilder<TStartup>(WebApplicationFactory<TStartup> factory)
    where TStartup : class
{
    private readonly List<Action<IServiceCollection>> _serviceRegistrations = [];
    private readonly List<Action<IConfigurationBuilder>> _configRegistrations = [];

    public WebApplicationFactory<TStartup> Build()
    {
        return factory.WithWebHostBuilder(builder =>
        {
            builder
                .UseIntegrationTestEnvironment()
                .UseTestServer()
                .ConfigureServices(services =>
                {
                    _serviceRegistrations.ForEach(registrations => registrations.Invoke(services));
                })
                .ConfigureAppConfiguration(
                    (_, config) =>
                    {
                        _configRegistrations.ForEach(registrations => registrations.Invoke(config));
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

    public OptimisedWebApplicationFactoryBuilder<TStartup> AddServiceRegistration(
        Action<IServiceCollection> serviceRegistration
    )
    {
        _serviceRegistrations.Add(serviceRegistration);
        return this;
    }

    public OptimisedWebApplicationFactoryBuilder<TStartup> AddConfigRegistration(
        Action<IConfigurationBuilder> configRegistration
    )
    {
        _configRegistrations.Add(configRegistration);
        return this;
    }

    public OptimisedWebApplicationFactoryBuilder<TStartup> WithTestUserAuthentication()
    {
        _serviceRegistrations.Add(RegisterTestUserAuthentication);
        return this;
    }

    public OptimisedWebApplicationFactoryBuilder<TStartup> WithServiceCollectionModification(
        OptimisedServiceCollectionModifications modifications
    )
    {
        _serviceRegistrations.AddRange(modifications.Actions);
        return this;
    }

    private static void RegisterTestUserAuthentication(IServiceCollection services)
    {
        services
            .AddSingleton<OptimisedTestUserPool>()
            .AddAuthentication("Bearer")
            .AddScheme<AuthenticationSchemeOptions, OptimisedTestAuthHandler>("Bearer", null);
    }
}

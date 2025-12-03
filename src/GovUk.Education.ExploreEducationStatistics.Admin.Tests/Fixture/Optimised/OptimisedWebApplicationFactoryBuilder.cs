#nullable enable
using GovUk.Education.ExploreEducationStatistics.Admin.Database;
using GovUk.Education.ExploreEducationStatistics.Admin.Services.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Admin.Services.Interfaces.Public.Data;
using GovUk.Education.ExploreEducationStatistics.Common.Extensions;
using GovUk.Education.ExploreEducationStatistics.Common.Services;
using GovUk.Education.ExploreEducationStatistics.Common.Services.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Common.Tests.Extensions;
using GovUk.Education.ExploreEducationStatistics.Content.Model.Database;
using GovUk.Education.ExploreEducationStatistics.Data.Model.Database;
using GovUk.Education.ExploreEducationStatistics.Data.Processor.Model;
using GovUk.Education.ExploreEducationStatistics.Public.Data.Model.Database;
using GovUk.Education.ExploreEducationStatistics.Publisher.Model;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Moq;
using Testcontainers.Azurite;
using Testcontainers.PostgreSql;

namespace GovUk.Education.ExploreEducationStatistics.Admin.Tests.Fixture.Optimised;

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

    public OptimisedWebApplicationFactoryBuilder<TStartup> WithReconfiguredAdmin()
    {
        _serviceRegistrations.Add(ReconfigureAdminServices);
        return this;
    }

    public OptimisedWebApplicationFactoryBuilder<TStartup> WithAzurite(AzuriteContainer testContainer)
    {
        _serviceRegistrations.Add(services => RegisterAzuriteServices(services, testContainer.GetConnectionString()));
        _configRegistrations.Add(config =>
            RegisterAzuriteAppConfiguration(config, testContainer.GetConnectionString())
        );
        return this;
    }

    public OptimisedWebApplicationFactoryBuilder<TStartup> WithPostgres(PostgreSqlContainer testContainer)
    {
        _serviceRegistrations.Add(services => RegisterPostgres(services, testContainer.GetConnectionString()));
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

    private void ReconfigureAdminServices(IServiceCollection services)
    {
        services
            .UseInMemoryDbContext<ContentDbContext>(databaseName: $"{nameof(ContentDbContext)}_{Guid.NewGuid()}")
            .UseInMemoryDbContext<StatisticsDbContext>(databaseName: $"{nameof(StatisticsDbContext)}_{Guid.NewGuid()}")
            .UseInMemoryDbContext<UsersAndRolesDbContext>(
                databaseName: $"{nameof(UsersAndRolesDbContext)}_{Guid.NewGuid()}"
            )
            .AddScoped<PublicDataDbContext>(_ => Mock.Of<PublicDataDbContext>(MockBehavior.Loose))
            .MockService<IProcessorClient>()
            .MockService<IPublicDataApiClient>()
            .MockService<IDataProcessorClient>()
            .MockService<IPublisherClient>()
            .MockService<IPublisherTableStorageService>()
            .MockService<IPrivateBlobStorageService>()
            .MockService<IPublicBlobStorageService>()
            .MockService<IAdminEventRaiser>(MockBehavior.Loose) // Ignore calls to publish events
            .RegisterControllers<Startup>();
    }

    private void RegisterPostgres(IServiceCollection services, string connectionString)
    {
        var descriptor = services.SingleOrDefault(d => d.ServiceType == typeof(PublicDataDbContext));

        if (descriptor != null)
        {
            services.Remove(descriptor);
        }

        services.AddDbContext<PublicDataDbContext>(options =>
            options
                // TODO EES-6450 - remove manual setting?
                .UseNpgsql(connectionString + ";Include Error Detail=true")
                .EnableSensitiveDataLogging()
                .EnableDetailedErrors()
        );

        using var serviceScope = services
            .BuildServiceProvider()
            .GetRequiredService<IServiceScopeFactory>()
            .CreateScope();

        using var context = serviceScope.ServiceProvider.GetRequiredService<PublicDataDbContext>();
        context.Database.Migrate();
    }

    private static void RegisterAzuriteAppConfiguration(IConfigurationBuilder config, string connectionString)
    {
        // TODO EES-6450 - what to do with this?
        // if (_azuriteContainer.State != TestcontainersStates.Running)
        // {
        //     throw new InvalidOperationException(
        //         $"Azurite container must be started via '{nameof(InitializeWithAzurite)}' method first");
        // }

        config.AddInMemoryCollection(
            [
                new KeyValuePair<string, string?>("PublicStorage", connectionString),
                new KeyValuePair<string, string?>("PublisherStorage", connectionString),
            ]
        );
    }

    private static void RegisterAzuriteServices(IServiceCollection services, string connectionString)
    {
        services.ReplaceService<IPublicBlobStorageService>(sp => new PublicBlobStorageService(
            connectionString,
            sp.GetRequiredService<ILogger<IBlobStorageService>>(),
            sp.GetRequiredService<IBlobSasService>()
        ));
        services.ReplaceService<IPrivateBlobStorageService>(sp => new PrivateBlobStorageService(
            connectionString,
            sp.GetRequiredService<ILogger<IBlobStorageService>>(),
            sp.GetRequiredService<IBlobSasService>()
        ));
        services.ReplaceService<IPublisherTableStorageService>(_ => new PublisherTableStorageService(connectionString));
        services.ReplaceService<IDataProcessorClient>(_ => new DataProcessorClient(connectionString));
        services.AddTransient<IPublicBlobCacheService, PublicBlobCacheService>();
        services.AddTransient<IPrivateBlobCacheService, PrivateBlobCacheService>();
    }

    private static void RegisterTestUserAuthentication(IServiceCollection services)
    {
        services
            .AddSingleton<OptimisedTestUserPool>()
            .AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
            .AddScheme<AuthenticationSchemeOptions, OptimisedTestAuthHandler>(
                JwtBearerDefaults.AuthenticationScheme,
                null
            );
    }
}

public static class OptimisedWebApplicationFactoryExtensions
{
    /// <summary>
    ///
    /// This is just a convenience method for registering Admin services so that
    /// it can be chained like:
    ///
    /// new WebApplicationFactory{Startup}()
    ///     .WithReconfiguredAdmin()
    ///     .WithSomething()
    ///     .WithSomethingElse();
    ///
    /// rather than:
    ///
    /// WithReconfiguredAdmin(new WebApplicationFactory{Startup}())
    ///     .WithSomething()
    ///     .WithSomethingElse();
    ///
    /// This changes the resulting chained response from a
    /// <see cref="WebApplicationFactory{TEntryPoint}"/>
    /// to an <see cref="OptimisedWebApplicationFactoryBuilder{TStartup}"/>.
    ///
    /// </summary>
    public static OptimisedWebApplicationFactoryBuilder<Startup> WithReconfiguredAdmin(
        this WebApplicationFactory<Startup> testApp
    )
    {
        return new OptimisedWebApplicationFactoryBuilder<Startup>(testApp).WithReconfiguredAdmin();
    }
}

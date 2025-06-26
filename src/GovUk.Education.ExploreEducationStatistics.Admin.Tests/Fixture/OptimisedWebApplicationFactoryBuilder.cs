#nullable enable
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Text.Encodings.Web;
using System.Threading.Tasks;
using GovUk.Education.ExploreEducationStatistics.Admin.Database;
using GovUk.Education.ExploreEducationStatistics.Admin.Services.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Common.Extensions;
using GovUk.Education.ExploreEducationStatistics.Common.Services;
using GovUk.Education.ExploreEducationStatistics.Common.Services.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Common.Tests.Extensions;
using GovUk.Education.ExploreEducationStatistics.Common.Tests.Fixtures;
using GovUk.Education.ExploreEducationStatistics.Content.Model.Database;
using GovUk.Education.ExploreEducationStatistics.Data.Model.Database;
using GovUk.Education.ExploreEducationStatistics.Data.Processor.Model;
using GovUk.Education.ExploreEducationStatistics.Public.Data.Model.Database;
using GovUk.Education.ExploreEducationStatistics.Publisher.Model;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Console;
using Microsoft.Extensions.Options;
using Moq;
using Testcontainers.Azurite;
using Testcontainers.PostgreSql;

namespace GovUk.Education.ExploreEducationStatistics.Admin.Tests.Fixture;

public class OptimisedWebApplicationFactoryBuilder<TStartup>(
    WebApplicationFactory<TStartup> factory) where TStartup : class
{
    private readonly List<Action<IServiceCollection>> _serviceRegistrations = [];
    private readonly List<Action<IConfigurationBuilder>> _configRegistrations = [];
    
    public WebApplicationFactory<TStartup> Build()
    {
        return factory.WithWebHostBuilder(builder =>
        {
            builder
                // .UseStartup<TStartup>()
                .UseIntegrationTestEnvironment()
                .UseTestServer()
                .ConfigureServices(services =>
                {
                    _serviceRegistrations.ForEach(registrations => registrations.Invoke(services));
                })
                .ConfigureAppConfiguration((_, config) =>
                {
                    _configRegistrations.ForEach(registrations => registrations.Invoke(config));
                })
                .ConfigureAppConfiguration(config =>
                {
                    config.AddConfiguration(new ConfigurationBuilder()
                        .AddJsonFile($"appsettings.{HostEnvironmentExtensions.IntegrationTestEnvironment}.json", optional: true)
                        .Build());
                });;
        });
    }
    
    // protected override IHostBuilder CreateHostBuilder()
    // {
    //     return Host
    //         .CreateDefaultBuilder()
    //         .ConfigureLogging(
    //             builder =>
    //             {
    //                 builder
    //                     .AddFilter<ConsoleLoggerProvider>("Default", LogLevel.Warning)
    //                     .AddFilter<ConsoleLoggerProvider>("Microsoft", LogLevel.Warning);
    //
    //                 // Uncomment to add SQL logging to the debug console.
    //                 // .AddFilter<ConsoleLoggerProvider>((category, level) =>
    //                 //     category == DbLoggerCategory.Database.Command.Name
    //                 //     && level == LogLevel.Information);
    //             }
    //         )
    //         .ConfigureWebHostDefaults(builder =>
    //         {
    //             // builder
    //             //     .UseStartup<TStartup>()
    //             //     .UseIntegrationTestEnvironment()
    //             //     .UseTestServer();
    //         })
    //         .ConfigureAppConfiguration(config =>
    //         {
    //             // config.AddConfiguration(new ConfigurationBuilder()
    //             //     .AddJsonFile($"appsettings.{IntegrationTestEnvironment}.json", optional: true)
    //             //     .Build());
    //         });
    // }
    
    public OptimisedWebApplicationFactoryBuilder<TStartup> WithAdmin()
    {
        _serviceRegistrations.Add(RegisterAdminServices);
        return this;
    }
    
    public OptimisedWebApplicationFactoryBuilder<TStartup> WithAzurite(
        AzuriteContainer testContainer)
    {
        _serviceRegistrations.Add(services => RegisterAzuriteServices(services, testContainer.GetConnectionString()));
        _configRegistrations.Add(config => RegisterAzuriteAppConfiguration(config, testContainer.GetConnectionString()));
        return this;
    }
    
    public OptimisedWebApplicationFactoryBuilder<TStartup> WithPostgres(
        PostgreSqlContainer testContainer)
    {
        _serviceRegistrations.Add(services => RegisterPostgres(services, testContainer.GetConnectionString()));
        return this;
    }
    
    private void RegisterAdminServices(IServiceCollection services)
    {
        services
            .UseInMemoryDbContext<ContentDbContext>()
            .UseInMemoryDbContext<StatisticsDbContext>()
            .UseInMemoryDbContext<UsersAndRolesDbContext>()
            .AddScoped<PublicDataDbContext>(_ => Mock.Of<PublicDataDbContext>(MockBehavior.Loose))
            .MockService<IDataProcessorClient>()
            .MockService<IPublisherClient>()
            .MockService<IPublisherTableStorageService>()
            .MockService<IPrivateBlobStorageService>()
            .MockService<IPublicBlobStorageService>()
            .MockService<IAdminEventRaiser>(MockBehavior.Loose) // Ignore calls to publish events
            .RegisterControllers<Startup>();

        services
            .AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
            .AddScheme<AuthenticationSchemeOptions, TestAuthHandler>(JwtBearerDefaults.AuthenticationScheme,
                null);
   }

    private void RegisterPostgres(
        IServiceCollection services,
        string connectionString)
    {
        var descriptor = services
            .SingleOrDefault(d => d.ServiceType == typeof(PublicDataDbContext));

        if (descriptor != null)
        {
            services.Remove(descriptor);
        }

        services.AddDbContext<PublicDataDbContext>(
            options => options.UseNpgsql(connectionString));

        using var serviceScope = services.BuildServiceProvider()
            .GetRequiredService<IServiceScopeFactory>()
            .CreateScope();

        using var context = serviceScope.ServiceProvider.GetRequiredService<PublicDataDbContext>();
        context.Database.Migrate();
    }

    private static void RegisterAzuriteAppConfiguration(
        IConfigurationBuilder config,
        string connectionString)
    {
        // if (_azuriteContainer.State != TestcontainersStates.Running)
        // {
        //     throw new InvalidOperationException(
        //         $"Azurite container must be started via '{nameof(InitializeWithAzurite)}' method first");
        // }


        config.AddInMemoryCollection(
        [
            new KeyValuePair<string, string?>("PublicStorage", connectionString),
            new KeyValuePair<string, string?>("PublisherStorage", connectionString),
        ]);
    }

    private static void RegisterAzuriteServices(
        IServiceCollection services,
        string connectionString)
    {
        services.ReplaceService<IPublicBlobStorageService>(sp =>
            new PublicBlobStorageService(
                connectionString,
                sp.GetRequiredService<ILogger<IBlobStorageService>>()
            )
        );
        services.ReplaceService<IPrivateBlobStorageService>(sp =>
            new PrivateBlobStorageService(
                connectionString,
                sp.GetRequiredService<ILogger<IBlobStorageService>>()
            )
        );
        services.ReplaceService<IPublisherTableStorageService>(_ =>
            new PublisherTableStorageService(connectionString)
        );
        services.ReplaceService<IDataProcessorClient>(_ =>
            new DataProcessorClient(connectionString)
        );
        services.AddTransient<IPublicBlobCacheService, PublicBlobCacheService>();
        services.AddTransient<IPrivateBlobCacheService, PrivateBlobCacheService>();
    }
    
    /// <summary>
    /// An AuthenticationHandler that allows the tests to make a ClaimsPrincipal available in the HttpContext
    /// for authentication and authorization mechanisms to use.
    /// </summary>
    internal class TestAuthHandler(
        IOptionsMonitor<AuthenticationSchemeOptions> options,
        ILoggerFactory logger,
        UrlEncoder encoder,
        IHttpContextAccessor httpContextAccessor)
        : AuthenticationHandler<AuthenticationSchemeOptions>(options, logger, encoder)
    {
        protected override Task<AuthenticateResult> HandleAuthenticateAsync()
        {
            if (!httpContextAccessor.HttpContext.TryGetRequestHeader("TestUser", out var fakeUserName))
            {
                return Task.FromResult(AuthenticateResult.NoResult());
            }

            if (!Enum.TryParse(typeof(TestUser), fakeUserName, out var testUser))
            {
                throw new ArgumentException($"{fakeUserName} is not a recognised test user");
            }

            var dataFixture = new DataFixture();

            ClaimsPrincipal user = testUser switch
            {
                TestUser.Bau => dataFixture.BauUser(),
                TestUser.Authenticated => dataFixture.AuthenticatedUser(),
                _ => throw new ArgumentException($"{fakeUserName} is not a recognised test user")
            };

            var ticket = new AuthenticationTicket(user, JwtBearerDefaults.AuthenticationScheme);
            var result = AuthenticateResult.Success(ticket);
            return Task.FromResult(result);
        }
    }

    public enum TestUser
    {
        Bau,
        Authenticated
    }
}

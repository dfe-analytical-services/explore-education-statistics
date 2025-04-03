#nullable enable
using System;
using Azure.Core;
using Azure.Identity;
using GovUk.Education.ExploreEducationStatistics.Common.Services.EventGrid;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Npgsql;
#pragma warning disable CS8974 // Converting method group to non-delegate type

namespace GovUk.Education.ExploreEducationStatistics.Common.Extensions;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddFunctionAppPsqlDbContext<TDbContext>(
        this IServiceCollection services,
        string connectionString,
        HostBuilderContext hostBuilderContext,
        Action<DbContextOptionsBuilder>? optionsConfiguration = null)
        where TDbContext : DbContext =>
        hostBuilderContext.HostingEnvironment.IsDevelopment()
            ? services.AddDevelopmentPsqlDbContext<TDbContext>(connectionString)
            : services.AddFunctionAppManagedIdentityPsqlDbContext<TDbContext>(
                connectionString,
                hostBuilderContext.Configuration,
                optionsConfiguration);

    public static IServiceCollection AddPsqlDbContext<TDbContext>(
        this IServiceCollection services,
        string connectionString,
        IHostEnvironment hostEnvironment,
        Action<DbContextOptionsBuilder>? optionsConfiguration = null)
        where TDbContext : DbContext =>
        hostEnvironment.IsDevelopment()
            ? services.AddDevelopmentPsqlDbContext<TDbContext>(connectionString)
            : services.AddManagedIdentityPsqlDbContext<TDbContext>(
                connectionString,
                optionsConfiguration);

    private static IServiceCollection AddFunctionAppManagedIdentityPsqlDbContext<TDbContext>(
        this IServiceCollection services,
        string connectionString,
        IConfiguration configuration,
        Action<DbContextOptionsBuilder>? optionsConfiguration = null)
        where TDbContext : DbContext
    {
        // Unlike Container Apps and App Services, DefaultAzureCredential does not pick up 
        // the "AZURE_CLIENT_ID" environment variable automatically when operating within
        // a Function App.  We therefore provide it manually.
        var clientId = configuration["AZURE_CLIENT_ID"];
        return services.RegisterManagedIdentityPsqlDbContext<TDbContext>(
            connectionString, clientId, optionsConfiguration);
    }

    private static IServiceCollection AddManagedIdentityPsqlDbContext<TDbContext>(
        this IServiceCollection services,
        string connectionString,
        Action<DbContextOptionsBuilder>? optionsConfiguration = null)
        where TDbContext : DbContext =>
        services.RegisterManagedIdentityPsqlDbContext<TDbContext>(
            connectionString, optionsConfiguration: optionsConfiguration);

    private static IServiceCollection AddDevelopmentPsqlDbContext<TDbContext>(
        this IServiceCollection services,
        string connectionString,
        Action<DbContextOptionsBuilder>? optionsConfiguration = null)
        where TDbContext : DbContext
    {
        var dataSourceBuilder = new NpgsqlDataSourceBuilder(connectionString);

        // Set up the data source outside the `AddDbContext` action as this
        // prevents `ManyServiceProvidersCreatedWarning` warnings due to EF
        // creating over 20 `IServiceProvider` instances.
        var dataSource = dataSourceBuilder.Build();

        services.AddDbContext<TDbContext>(options =>
        {
            options
                .UseNpgsql(
                    dataSource,
                    psqlOptions => psqlOptions.EnableRetryOnFailure())
                .EnableSensitiveDataLogging();

            optionsConfiguration?.Invoke(options);
        });

        return services;
    }

    private static IServiceCollection RegisterManagedIdentityPsqlDbContext<TDbContext>(
        this IServiceCollection services,
        string connectionString,
        string? managedIdentityClientId = null,
        Action<DbContextOptionsBuilder>? optionsConfiguration = null)
        where TDbContext : DbContext
    {
        var accessTokenProvider = managedIdentityClientId != null
            ? new DefaultAzureCredential(
                new DefaultAzureCredentialOptions {ManagedIdentityClientId = managedIdentityClientId})
            : new DefaultAzureCredential();

        var dataSource = new NpgsqlDataSourceBuilder(connectionString)
            .UsePeriodicPasswordProvider(async (_, cancellationToken) =>
            {
                var tokenResponse = await accessTokenProvider
                    .GetTokenAsync(new TokenRequestContext([
                        "https://ossrdbms-aad.database.windows.net/.default"
                    ]), cancellationToken);

                return tokenResponse.Token;
            }, TimeSpan.FromMinutes(1), TimeSpan.FromSeconds(5))
            .Build();

        services.AddDbContext<TDbContext>(options =>
        {
            options
                .UseNpgsql(
                    dataSource,
                    psqlOptions => psqlOptions.EnableRetryOnFailure());

            optionsConfiguration?.Invoke(options);
        });

        return services;
    }

    public static IServiceCollection AddEventGridClient(this IServiceCollection services, IConfiguration configuration) =>
        services
            .AddTransient<IEventGridClientFactory, EventGridClientFactory>()
            .AddTransient<IConfiguredEventGridClientFactory, ConfiguredEventGridClientFactory>()
            .AddTransient(typeof(Func<ILogger<SafeEventGridClient>>), sp => sp.GetRequiredService<ILogger<SafeEventGridClient>>)
            .Configure<EventGridOptions>(configuration.GetSection(EventGridOptions.Section))
        ;
}

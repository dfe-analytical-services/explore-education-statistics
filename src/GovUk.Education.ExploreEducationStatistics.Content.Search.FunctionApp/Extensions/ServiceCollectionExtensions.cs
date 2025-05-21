using GovUk.Education.ExploreEducationStatistics.Content.Search.FunctionApp.Clients.AzureBlobStorage;
using GovUk.Education.ExploreEducationStatistics.Content.Search.FunctionApp.Clients.AzureSearch;
using GovUk.Education.ExploreEducationStatistics.Content.Search.FunctionApp.Clients.ContentApi;
using GovUk.Education.ExploreEducationStatistics.Content.Search.FunctionApp.Functions.HealthChecks.Strategies;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Azure;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace GovUk.Education.ExploreEducationStatistics.Content.Search.FunctionApp.Extensions;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddAzureClientsInline(
        this IServiceCollection serviceCollection,
        Action<AzureClientFactoryBuilder> builder)
    {
        serviceCollection.AddAzureClients(builder);
        return serviceCollection;
    }

    public static IServiceCollection AddHealthChecks(this IServiceCollection serviceCollection) =>
        serviceCollection
            .AddTransient<IHealthCheckStrategy, ContentApiHealthCheckStrategy>()
            .AddTransient<IHealthCheckStrategy, AzureBlobStorageHealthCheckStrategy>()
            .AddTransient<IHealthCheckStrategy, AzureSearchHealthCheckStrategy>()
            // Factories to allow Health check to evaluate config before attempting to instantiate clients
            .AddTransient<Func<IContentApiClient>>(sp => sp.GetRequiredService<IContentApiClient>)
            .AddTransient<Func<IAzureBlobStorageClient>>(sp => sp.GetRequiredService<IAzureBlobStorageClient>)
            .AddTransient<Func<ISearchIndexerClient>>(sp => sp.GetRequiredService<ISearchIndexerClient>);

    public static IServiceCollection ConfigureLogging(
        this IServiceCollection serviceCollection,
        IConfiguration configuration) =>
        serviceCollection
            .AddApplicationInsightsTelemetryWorkerService()
            .ConfigureFunctionsApplicationInsights()
            .Configure<LoggerFilterOptions>(
                options =>
                {
                    // The Application Insights SDK adds a default logging filter that instructs ILogger to capture
                    // only Warning and more severe logs. Application Insights requires an explicit override.
                    // Log levels can also be configured using appsettings.json.
                    // For more information, see https://learn.microsoft.com/en-us/azure/azure-monitor/app/worker-service#ilogger-logs
                    var toRemove = options.Rules.FirstOrDefault(
                        rule =>
                            rule.ProviderName ==
                            "Microsoft.Extensions.Logging.ApplicationInsights.ApplicationInsightsLoggerProvider");

                    if (toRemove is not null)
                    {
                        options.Rules.Remove(toRemove);
                    }
                });
}

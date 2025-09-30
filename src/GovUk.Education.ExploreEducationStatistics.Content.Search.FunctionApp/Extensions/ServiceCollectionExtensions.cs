using GovUk.Education.ExploreEducationStatistics.Content.Search.FunctionApp.Clients.AzureBlobStorage;
using GovUk.Education.ExploreEducationStatistics.Content.Search.FunctionApp.Clients.AzureSearch;
using GovUk.Education.ExploreEducationStatistics.Content.Search.FunctionApp.Clients.ContentApi;
using GovUk.Education.ExploreEducationStatistics.Content.Search.FunctionApp.Functions.HealthChecks.Strategies;
using GovUk.Education.ExploreEducationStatistics.Content.Search.FunctionApp.Services.CheckSearchableDocuments;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Azure;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Serilog;

namespace GovUk.Education.ExploreEducationStatistics.Content.Search.FunctionApp.Extensions;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddAzureClientsInline(
        this IServiceCollection serviceCollection,
        Action<AzureClientFactoryBuilder> builder
    )
    {
        serviceCollection.AddAzureClients(builder);
        return serviceCollection;
    }

    public static IServiceCollection AddHealthChecks(this IServiceCollection serviceCollection) =>
        serviceCollection
            .AddTransient<IHealthCheckStrategy, ContentApiHealthCheckStrategy>()
            .AddTransient<IHealthCheckStrategy, AzureBlobStorageHealthCheckStrategy>()
            .AddTransient<IHealthCheckStrategy, AzureSearchHealthCheckStrategy>()
            .AddTransient<IHealthCheckStrategy, LoggingHealthCheck>()
            // Factories to allow Health check to evaluate config before attempting to instantiate clients
            .AddTransient<Func<IContentApiClient>>(sp => sp.GetRequiredService<IContentApiClient>)
            .AddTransient<Func<IAzureBlobStorageClient>>(sp =>
                sp.GetRequiredService<IAzureBlobStorageClient>
            )
            .AddTransient<Func<ISearchIndexerClient>>(sp =>
                sp.GetRequiredService<ISearchIndexerClient>
            );

    public static IServiceCollection AddSearchDocumentChecker(
        this IServiceCollection serviceCollection
    ) =>
        serviceCollection
            .AddTransient<SearchableDocumentChecker>()
            .AddTransient<IReleaseSummaryRetriever, ReleaseSummaryRetriever>()
            .AddTransient<IBlobNameLister, BlobNameLister>();

    public static IServiceCollection ConfigureLogging(
        this IServiceCollection serviceCollection,
        IConfiguration configuration
    ) =>
        serviceCollection
            .SetupAppInsights()
            .AddSerilog(loggerConfiguration =>
                loggerConfiguration.ConfigureSerilogLogger(configuration)
            );

    private static IServiceCollection SetupAppInsights(this IServiceCollection serviceCollection) =>
        serviceCollection
            // Setup App Insights, so that metrics are recorded
            .AddApplicationInsightsTelemetryWorkerService()
            .ConfigureFunctionsApplicationInsights()
            .RemoveAppInsightsLoggingLevelRestriction();

    private static IServiceCollection RemoveAppInsightsLoggingLevelRestriction(
        this IServiceCollection serviceCollection
    ) =>
        serviceCollection.Configure<LoggerFilterOptions>(options =>
        {
            // The Application Insights SDK adds a default logging filter that instructs ILogger to capture
            // only Warning and more severe logs. Application Insights requires an explicit override.
            // Log levels can also be configured using appsettings.json.
            // For more information, see https://learn.microsoft.com/en-us/azure/azure-monitor/app/worker-service#ilogger-logs
            var defaultRule = options.Rules.FirstOrDefault(rule =>
                rule.ProviderName
                == "Microsoft.Extensions.Logging.ApplicationInsights.ApplicationInsightsLoggerProvider"
            );

            if (defaultRule is not null)
            {
                options.Rules.Remove(defaultRule);
            }
        });
}

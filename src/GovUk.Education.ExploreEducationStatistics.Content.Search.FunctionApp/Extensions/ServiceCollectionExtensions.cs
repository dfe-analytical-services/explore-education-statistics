using GovUk.Education.ExploreEducationStatistics.Content.Search.FunctionApp.Clients.AzureBlobStorage;
using GovUk.Education.ExploreEducationStatistics.Content.Search.FunctionApp.Clients.AzureSearch;
using GovUk.Education.ExploreEducationStatistics.Content.Search.FunctionApp.Clients.ContentApi;
using GovUk.Education.ExploreEducationStatistics.Content.Search.FunctionApp.Functions.HealthChecks.Strategies;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Azure;
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
        this IServiceCollection serviceCollection) =>
        serviceCollection
            .SetupAppInsights()
        ;

    private static IServiceCollection SetupAppInsights(this IServiceCollection serviceCollection) =>
        serviceCollection
            // Setup App Insights, so that metrics are recorded
            .AddApplicationInsightsTelemetryWorkerService()
            .ConfigureFunctionsApplicationInsights()
            .RemoveAppInsightsLoggingLevelRestriction();

    private static IServiceCollection RemoveAppInsightsLoggingLevelRestriction(this IServiceCollection serviceCollection) =>
        serviceCollection
            .Configure<LoggerFilterOptions>(
                options =>
                {
                    var defaultRule = options.Rules.FirstOrDefault(
                        rule => rule.ProviderName ==
                                "Microsoft.Extensions.Logging.ApplicationInsights.ApplicationInsightsLoggerProvider");
                    
                    if (defaultRule is not null)
                    {
                        options.Rules.Remove(defaultRule);
                    }
                });
}

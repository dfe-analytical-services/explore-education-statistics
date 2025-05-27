using GovUk.Education.ExploreEducationStatistics.Content.Search.FunctionApp.Clients.AzureBlobStorage;
using GovUk.Education.ExploreEducationStatistics.Content.Search.FunctionApp.Clients.AzureSearch;
using GovUk.Education.ExploreEducationStatistics.Content.Search.FunctionApp.Clients.ContentApi;
using GovUk.Education.ExploreEducationStatistics.Content.Search.FunctionApp.Functions.HealthChecks.Strategies;
using Microsoft.ApplicationInsights.Extensibility;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Azure;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Serilog;
using Serilog.Exceptions;

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
            .SetupAppInsights()
            .AddLogging(lb =>
            {
                // Prevent the default Azure Function logging provider from logging.
                // Instead, let Serilog handle that.
                lb.SetMinimumLevel(LogLevel.None);

                // Resolve the telemetry configuration that was registered by SetupAppInsights.
                // Note: It's a bit naughty resolving it here and potentially might not be ready.
                var telemetryConfiguration = serviceCollection.BuildServiceProvider().GetRequiredService<TelemetryConfiguration>();

                // Setup Serilog to log to the Console and to App Insights
                lb.AddSerilog(
                    new LoggerConfiguration()
                        .ReadFrom.Configuration(configuration)
                        .WriteTo.ApplicationInsights(telemetryConfiguration, TelemetryConverter.Traces)
                        .CreateLogger(),
                    dispose: true
                );
            })
        ;

    private static IServiceCollection SetupAppInsights(this IServiceCollection serviceCollection) =>
        serviceCollection
            // Setup App Insights, so that metrics are recorded
            .AddApplicationInsightsTelemetryWorkerService()
            .ConfigureFunctionsApplicationInsights()
            .PreventDefaultAppInsightsLogging();

    private static IServiceCollection PreventDefaultAppInsightsLogging(this IServiceCollection serviceCollection) =>
        serviceCollection
            .Configure<LoggerFilterOptions>(
                options =>
                {
                    // The Application Insights SDK adds a default logging filter that instructs ILogger to capture
                    // only Warning and more severe logs. Application Insights requires an explicit override.
                    
                    // Find the built-in App Insights logging rule
                    var defaultRule = options.Rules.FirstOrDefault(
                        rule => rule.ProviderName ==
                                "Microsoft.Extensions.Logging.ApplicationInsights.ApplicationInsightsLoggerProvider");

                    if (defaultRule is not null)
                    {
                        // Remove the default rule
                        options.Rules.Remove(defaultRule);

                        // Create a new rule, manually setting the log level to None
                        // to prevent the function from logging to App Insights.
                        // Let Serilog handle the logging.
                        options.Rules.Add(
                            new LoggerFilterRule(
                                defaultRule.ProviderName,
                                defaultRule.CategoryName,
                                LogLevel.None,
                                defaultRule.Filter
                            ));
                    }
                });
}

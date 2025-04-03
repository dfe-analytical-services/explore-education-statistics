using Azure.Identity;
using Azure.Storage.Blobs;
using GovUk.Education.ExploreEducationStatistics.Content.Search.FunctionApp.Clients.AzureBlobStorage;
using GovUk.Education.ExploreEducationStatistics.Content.Search.FunctionApp.Clients.AzureSearch;
using GovUk.Education.ExploreEducationStatistics.Content.Search.FunctionApp.Clients.ContentApi;
using GovUk.Education.ExploreEducationStatistics.Content.Search.FunctionApp.Options;
using GovUk.Education.ExploreEducationStatistics.Content.Search.FunctionApp.Services;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.Net.Http.Headers;

namespace GovUk.Education.ExploreEducationStatistics.Content.Search.FunctionApp.Extensions;

public static class HostBuilderExtension
{
    public static IHost BuildHost(this IHostBuilder hostBuilder) => hostBuilder
        .ConfigureFunctionsWebApplication()
        .ConfigureAppConfiguration(
            (context, configurationBuilder) =>
                configurationBuilder
                    .AddJsonFile($"appsettings.json", true, false)
                    .AddJsonFile(
                        $"appsettings.{context.HostingEnvironment.EnvironmentName}.json",
                        true,
                        false)
                    .AddEnvironmentVariables())
        .ConfigureServices(
            (context, services) =>
                services
                    .AddApplicationInsightsTelemetryWorkerService()
                    .ConfigureFunctionsApplicationInsights()
                    // Config
                    .Configure<AppOptions>(context.Configuration.GetSection(AppOptions.Section))
                    .Configure<ContentApiOptions>(context.Configuration.GetSection(ContentApiOptions.Section))
                    .Configure<AzureSearchOptions>(context.Configuration.GetSection(AzureSearchOptions.Section))
                    .Configure<LoggerFilterOptions>(options =>
                    {
                        // The Application Insights SDK adds a default logging filter that instructs ILogger to capture
                        // only Warning and more severe logs. Application Insights requires an explicit override.
                        // Log levels can also be configured using appsettings.json.
                        // For more information, see https://learn.microsoft.com/en-us/azure/azure-monitor/app/worker-service#ilogger-logs
                        var toRemove = options.Rules.FirstOrDefault(rule =>
                            rule.ProviderName ==
                            "Microsoft.Extensions.Logging.ApplicationInsights.ApplicationInsightsLoggerProvider");

                        if (toRemove is not null)
                        {
                            options.Rules.Remove(toRemove);
                        }
                    })
                    // Services
                    .AddTransient<ISearchableDocumentCreator, SearchableDocumentCreator>()
                    // Functions
                    .AddTransient<IEventGridEventHandler, EventGridEventHandler>()
                    .AddHealthChecks()
                    // Clients
                    .AddTransient<ISearchIndexClient, SearchIndexClient>()
                    .AddTransient<IAzureSearchIndexerClientFactory, AzureSearchIndexerClientFactory>()
                    .AddTransient<IAzureBlobStorageClient, AzureBlobStorageClient>()
                    .AddAzureClientsInline(
                        clientBuilder =>
                        {
                            clientBuilder.AddClient<BlobServiceClient, BlobClientOptions>(
                                (_, _, serviceProvider) =>
                                {
                                    var appOptions = serviceProvider.GetRequiredService<IOptions<AppOptions>>().Value;
                                    return new BlobServiceClient(appOptions.SearchStorageConnectionString);
                                });
                            clientBuilder.UseCredential(new DefaultAzureCredential());
                        })
                    .AddHttpClient<IContentApiClient, ContentApiClient>(
                        (provider, httpClient) =>
                        {
                            var options = provider.GetRequiredService<IOptions<ContentApiOptions>>().Value;
                            httpClient.BaseAddress = new Uri(options.Url);
                            httpClient.DefaultRequestHeaders.Add(
                                HeaderNames.UserAgent,
                                "EES Content Search Function App");
                        })
        )
        .Build();
}

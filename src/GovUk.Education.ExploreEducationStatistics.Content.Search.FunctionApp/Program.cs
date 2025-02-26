using System.Configuration;
using Azure.Identity;
using Azure.Storage.Blobs;
using GovUk.Education.ExploreEducationStatistics.Content.Search.FunctionApp.Clients;
using GovUk.Education.ExploreEducationStatistics.Content.Search.FunctionApp.Clients.AzureBlobStorage;
using GovUk.Education.ExploreEducationStatistics.Content.Search.FunctionApp.Clients.ContentApi;
using GovUk.Education.ExploreEducationStatistics.Content.Search.FunctionApp.Extensions;
using GovUk.Education.ExploreEducationStatistics.Content.Search.FunctionApp.Options;
using GovUk.Education.ExploreEducationStatistics.Content.Search.FunctionApp.Services;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;
using Microsoft.Net.Http.Headers;

namespace GovUk.Education.ExploreEducationStatistics.Content.Search.FunctionApp;

internal class Program
{
    public static async Task Main(string[] _)
    {
        await BuildHost().RunAsync();
    }
    
    public static IHost BuildHost(IHostBuilder? hostBuilder = null) => (hostBuilder ?? new HostBuilder())
        .ConfigureFunctionsWebApplication()
        .ConfigureAppConfiguration((context, configurationBuilder) =>
            configurationBuilder
                .AddJsonFile(path: $"appsettings.json", optional: true, reloadOnChange: false)
                .AddJsonFile(path: $"appsettings.{context.HostingEnvironment.EnvironmentName}.json",
                    optional: true,
                    reloadOnChange: false)
                .AddEnvironmentVariables())
        .ConfigureServices((context, services) =>
            services
                .AddApplicationInsightsTelemetryWorkerService()
                .ConfigureFunctionsApplicationInsights()

                .Configure<AppOptions>(context.Configuration.GetSection(AppOptions.Section))
                .Configure<ContentApiOptions>(context.Configuration.GetSection(ContentApiOptions.Section))

                .AddTransient<IContentApiClient, ContentApiClient>()
                .AddTransient<IAzureBlobStorageClient, AzureBlobStorageClient>()
                .AddTransient<ISearchableDocumentCreator, SearchableDocumentCreator>()
                .AddAzureClientsInline(clientBuilder =>
                {
                    clientBuilder.AddClient<BlobServiceClient, BlobClientOptions>((_, _, serviceProvider) =>
                    {
                        var appOptions = serviceProvider.GetRequiredService<IOptions<AppOptions>>().Value;
                        appOptions.Validate();
                        return new BlobServiceClient(appOptions.SearchStorageConnectionString);
                    });
                    clientBuilder.UseCredential(new DefaultAzureCredential());
                })
                .AddHttpClient<IContentApiClient, ContentApiClient>((provider, httpClient) =>
                {
                    var options = provider.GetRequiredService<IOptions<ContentApiOptions>>().Value;
                    options.Validate();

                    httpClient.BaseAddress = new Uri(options.Url);
                    httpClient.DefaultRequestHeaders.Add(HeaderNames.UserAgent, "EES Content Search Function App");
                })

        )
        .Build()
        .Execute(host =>
        {
            // Validate the configuration on startup to fail fast.
            host.Services.GetRequiredService<IOptions<AppOptions>>().Value.Validate();
            host.Services.GetRequiredService<IOptions<ContentApiOptions>>().Value.Validate();
        });
}

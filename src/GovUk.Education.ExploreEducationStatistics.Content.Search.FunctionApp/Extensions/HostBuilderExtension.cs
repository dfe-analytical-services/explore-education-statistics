using Azure.Identity;
using Azure.Storage.Blobs;
using GovUk.Education.ExploreEducationStatistics.Content.Search.FunctionApp.Clients.AzureBlobStorage;
using GovUk.Education.ExploreEducationStatistics.Content.Search.FunctionApp.Clients.AzureSearch;
using GovUk.Education.ExploreEducationStatistics.Content.Search.FunctionApp.Clients.ContentApi;
using GovUk.Education.ExploreEducationStatistics.Content.Search.FunctionApp.Options;
using GovUk.Education.ExploreEducationStatistics.Content.Search.FunctionApp.Services.Core;
using GovUk.Education.ExploreEducationStatistics.Content.Search.FunctionApp.Services.CreateSearchableDocuments;
using GovUk.Education.ExploreEducationStatistics.Content.Search.FunctionApp.Services.RemoveSearchableDocument;
using GovUk.Education.ExploreEducationStatistics.Content.Search.FunctionApp.Services.ResetSearchableDocuments;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;
using Microsoft.Net.Http.Headers;
using Serilog;

namespace GovUk.Education.ExploreEducationStatistics.Content.Search.FunctionApp.Extensions;

public static class HostBuilderExtension
{
    public static IHost BuildHost(this IHostBuilder hostBuilder) => hostBuilder
        .ConfigureFunctionsWebApplication()
        .ConfigureAppConfiguration(
            (context, configurationBuilder) =>
                configurationBuilder
                    .AddJsonFileAndLog("appsettings.json", false, false)
                    .AddJsonFileAndLog(
                        $"appsettings.{context.HostingEnvironment.EnvironmentName}.json",
                        false,
                        false)
                    .AddEnvironmentVariables())
        .ConfigureServices(
            (context, services) =>
                services
                    .ConfigureLogging(context.Configuration)
                    // Config
                    .Configure<AppOptions>(context.Configuration.GetSection(AppOptions.Section))
                    .Configure<ContentApiOptions>(context.Configuration.GetSection(ContentApiOptions.Section))
                    .Configure<AzureSearchOptions>(context.Configuration.GetSection(AzureSearchOptions.Section))
                    // Services
                    .AddTransient<ISearchableDocumentCreator, SearchableDocumentCreator>()
                    .AddTransient<ISearchableDocumentRemover, SearchableDocumentRemover>()
                    .AddTransient<IFullSearchableDocumentResetter, FullSearchableDocumentResetter>()
                    // Functions
                    .AddTransient<IEventGridEventHandler, EventGridEventHandler>()
                    .AddTransient<ICommandHandler, CommandHandler>()
                    .AddHealthChecks()
                    // Clients
                    .AddTransient<ISearchIndexerClient, SearchIndexerClient>()
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

    public static IHostBuilder InitialiseSerilog(this IHostBuilder hostBuilder)
    {
        // Setup Serilog
        // https://github.com/serilog/serilog-aspnetcore
        Log.Logger = new LoggerConfiguration()
            // Because we can't access appsettings before creating the HostBuilder we'll use a bootstrap logger
            // without configuration specific initialization first and replace it after the HostBuilder was created.
            // See https://github.com/serilog/serilog-aspnetcore#two-stage-initialization
            .ConfigureBootstrapLogger()
            .CreateBootstrapLogger();
        
        return hostBuilder;
    }
}

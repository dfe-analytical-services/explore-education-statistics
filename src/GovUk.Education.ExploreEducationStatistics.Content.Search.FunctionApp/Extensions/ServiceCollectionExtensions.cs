using GovUk.Education.ExploreEducationStatistics.Content.Search.FunctionApp.Clients.AzureBlobStorage;
using GovUk.Education.ExploreEducationStatistics.Content.Search.FunctionApp.Clients.AzureSearch;
using GovUk.Education.ExploreEducationStatistics.Content.Search.FunctionApp.Clients.ContentApi;
using GovUk.Education.ExploreEducationStatistics.Content.Search.FunctionApp.Functions.HealthChecks.Strategies;
using Microsoft.Extensions.Azure;
using Microsoft.Extensions.DependencyInjection;

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
            // Factories to allow Healthcheck to evaluate config before attempting to instantiate clients
            .AddTransient<Func<IContentApiClient>>(sp => sp.GetRequiredService<IContentApiClient>) 
            .AddTransient<Func<IAzureBlobStorageClient>>(sp => sp.GetRequiredService<IAzureBlobStorageClient>)
            .AddTransient<Func<ISearchIndexClient>>(sp => sp.GetRequiredService<ISearchIndexClient>)
        ;
}

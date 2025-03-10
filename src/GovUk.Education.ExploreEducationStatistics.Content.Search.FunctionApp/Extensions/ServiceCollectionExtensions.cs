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
            .AddTransient<IHeathCheckStrategy, ContentApiHealthCheckStrategy>()
            .AddTransient<IHeathCheckStrategy, AzureBlobStorageHealthCheckStrategy>();
}

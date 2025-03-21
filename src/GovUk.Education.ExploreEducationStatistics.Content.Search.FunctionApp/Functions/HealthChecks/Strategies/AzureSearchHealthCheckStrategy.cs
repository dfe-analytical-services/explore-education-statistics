using GovUk.Education.ExploreEducationStatistics.Content.Search.FunctionApp.Clients.AzureSearch;
using GovUk.Education.ExploreEducationStatistics.Content.Search.FunctionApp.Options;
using Microsoft.Extensions.Options;

namespace GovUk.Education.ExploreEducationStatistics.Content.Search.FunctionApp.Functions.HealthChecks.Strategies;

internal class AzureSearchHealthCheckStrategy(
    Func<ISearchIndexClient> searchIndexClientFactory,
    IOptions<AzureSearchOptions> azureSearchOptions) : IHealthCheckStrategy
{
    public async Task<HealthCheckResult> Run(CancellationToken cancellationToken)
    {
        if (!azureSearchOptions.Value.IsValid(out var errorMessage))
        {
            return new HealthCheckResult(false,  errorMessage);
        }
        
        try
        {
            var client = searchIndexClientFactory();
            if (!await client.IndexerExists(cancellationToken))
            {
                return new HealthCheckResult(false, $"Azure Search Indexer is not found");
            }

        }
        catch (Exception e)
        {
            return new HealthCheckResult(false, $"Error occurred whilst trying to run healthcheck for Azure Search Indexer: {e.Message}");
        }
        
        return new HealthCheckResult(true, "Connection to Azure Search Indexer:OK");
    }
}

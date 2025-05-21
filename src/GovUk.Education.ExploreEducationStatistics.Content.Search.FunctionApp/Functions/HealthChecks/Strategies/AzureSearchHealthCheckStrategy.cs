using GovUk.Education.ExploreEducationStatistics.Content.Search.FunctionApp.Clients.AzureSearch;
using GovUk.Education.ExploreEducationStatistics.Content.Search.FunctionApp.Options;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace GovUk.Education.ExploreEducationStatistics.Content.Search.FunctionApp.Functions.HealthChecks.Strategies;

internal class AzureSearchHealthCheckStrategy(
    Func<ISearchIndexerClient> searchIndexerClientFactory,
    ILogger<AzureBlobStorageHealthCheckStrategy> logger,
    IOptions<AzureSearchOptions> azureSearchOptions) : IHealthCheckStrategy
{
    public string Description => "Azure Search Indexer check";
    public async Task<HealthCheckResult> Run(CancellationToken cancellationToken)
    {
        logger.LogInformation("Running Azure Search health check");
        if (!azureSearchOptions.Value.IsValid(out var errorMessage))
        {
            logger.LogWarning("Azure Search health check failed: Provider options are not valid. {@Options}", azureSearchOptions.Value);
            return new HealthCheckResult(this, false, errorMessage);
        }

        try
        {
            logger.LogInformation("Attempting to connect to Azure Search Indexer. {@Options}...", azureSearchOptions.Value);
            var client = searchIndexerClientFactory();
            if (!await client.IndexerExists(cancellationToken))
            {
                logger.LogWarning("Azure Search Indexer is not found.");
                return new HealthCheckResult(this, false, $"Azure Search Indexer is not found");
            }
        }
        catch (Exception e)
        {
            logger.LogWarning(e, "Could not connect to Azure Search Indexer: {Message}", e.Message);
            return new HealthCheckResult(this, false, $"Error occurred whilst trying to run healthcheck for Azure Search Indexer: {e.Message}");
        }

        logger.LogInformation("Healthcheck was successful: Azure Search Indexer is found.");
        return new HealthCheckResult(this, true, "Connection to Azure Search Indexer:OK");
    }
}

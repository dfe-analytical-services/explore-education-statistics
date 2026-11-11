using GovUk.Education.ExploreEducationStatistics.Content.Search.FunctionApp.Clients.AzureSearch;
using GovUk.Education.ExploreEducationStatistics.Content.Search.FunctionApp.Options;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace GovUk.Education.ExploreEducationStatistics.Content.Search.FunctionApp.Functions.HealthChecks.Strategies;

internal class AzureSearchHealthCheckStrategy(
    Func<ISearchIndexerClient> searchIndexerClientFactory,
    ILogger<AzureSearchHealthCheckStrategy> logger,
    IOptions<AzureSearchOptions> azureSearchOptions
) : IHealthCheckStrategy
{
    public string Description => "Azure Search Indexers check";

    public async Task<HealthCheckResult> Run(CancellationToken cancellationToken)
    {
        logger.LogInformation("Running Azure Search health check");
        if (!azureSearchOptions.Value.IsValid(out var errorMessage))
        {
            logger.LogWarning(
                "Azure Search health check failed: Provider options are not valid. {@Options}",
                azureSearchOptions.Value
            );
            return new HealthCheckResult(this, false, errorMessage);
        }

        (string Description, string IndexerName)[] indexers =
        [
            ("Release", azureSearchOptions.Value.ReleaseIndexerName),
            ("Dataset", azureSearchOptions.Value.DatasetIndexerName),
        ];

        var client = searchIndexerClientFactory();

        var results = new List<(bool IsHealthy, string Message)>();
        foreach (var (description, indexerName) in indexers)
        {
            results.Add(await CheckIndexer(client, description, indexerName, cancellationToken));
        }

        return new HealthCheckResult(
            this,
            results.All(result => result.IsHealthy),
            string.Join(" ", results.Select(result => result.Message))
        );
    }

    private async Task<(bool IsHealthy, string Message)> CheckIndexer(
        ISearchIndexerClient client,
        string description,
        string indexerName,
        CancellationToken cancellationToken
    )
    {
        try
        {
            logger.LogInformation(
                "Attempting to connect to {Description} Azure Search Indexer '{IndexerName}'...",
                description,
                indexerName
            );
            if (!await client.IndexerExists(indexerName, cancellationToken))
            {
                logger.LogWarning(
                    "{Description} Azure Search Indexer '{IndexerName}' is not found.",
                    description,
                    indexerName
                );
                return (false, $"{description} Azure Search Indexer '{indexerName}' is not found.");
            }
        }
        catch (Exception e)
        {
            logger.LogWarning(
                e,
                "Could not connect to {Description} Azure Search Indexer '{IndexerName}': {Message}",
                description,
                indexerName,
                e.Message
            );
            return (
                false,
                $"Error occurred whilst trying to run health check for {description} Azure Search Indexer '{indexerName}': {e.Message}"
            );
        }

        logger.LogInformation(
            "Health check was successful: {Description} Azure Search Indexer '{IndexerName}' is found.",
            description,
            indexerName
        );
        return (true, $"Connection to {description} Azure Search Indexer '{indexerName}':OK.");
    }
}

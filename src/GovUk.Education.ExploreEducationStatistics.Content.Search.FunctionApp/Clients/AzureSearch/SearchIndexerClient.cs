using Microsoft.Extensions.Logging;

namespace GovUk.Education.ExploreEducationStatistics.Content.Search.FunctionApp.Clients.AzureSearch;

/// <summary>
/// Client for calling Azure Search Indexers
/// </summary>
public class SearchIndexerClient(
    IAzureSearchIndexerClientFactory azureSearchIndexerClientFactory,
    ILogger<SearchIndexerClient> logger
) : ISearchIndexerClient
{
    private const int IndexerAlreadyRunning = 409;

    public async Task RunIndexer(string indexerName, CancellationToken cancellationToken = default)
    {
        var client = azureSearchIndexerClientFactory.Create();
        try
        {
            var isIndexerRunning = await client.IsIndexerRunningAsync(indexerName, cancellationToken);
            if (!isIndexerRunning)
            {
                await client.RunIndexerAsync(indexerName, cancellationToken);
                logger.LogDebug("Indexer {IndexerName} has been started.", indexerName);
            }
            else
            {
                logger.LogDebug("Indexer {IndexerName} is already running.", indexerName);
            }
        }
        catch (Azure.RequestFailedException e) when (e.Status == IndexerAlreadyRunning)
        {
            logger.LogWarning("Can not start Indexer {IndexerName} because it is already running.", indexerName);
        }
    }

    public async Task<bool> IsIndexerRunning(string indexerName, CancellationToken cancellationToken = default)
    {
        var client = azureSearchIndexerClientFactory.Create();
        return await client.IsIndexerRunningAsync(indexerName, cancellationToken);
    }

    public async Task ResetIndexer(string indexerName, CancellationToken cancellationToken = default)
    {
        var client = azureSearchIndexerClientFactory.Create();
        await client.ResetIndexerAsync(indexerName, cancellationToken);
    }

    public async Task<bool> IndexerExists(string indexerName, CancellationToken cancellationToken = default)
    {
        var client = azureSearchIndexerClientFactory.Create();
        return await client.IndexerExists(indexerName, cancellationToken);
    }
}

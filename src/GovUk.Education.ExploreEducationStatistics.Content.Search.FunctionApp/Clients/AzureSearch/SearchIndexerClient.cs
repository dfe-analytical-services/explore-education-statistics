using GovUk.Education.ExploreEducationStatistics.Content.Search.FunctionApp.Options;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace GovUk.Education.ExploreEducationStatistics.Content.Search.FunctionApp.Clients.AzureSearch;

/// <summary>
/// Client for calling the configured Azure Search Indexer
/// </summary>
public class SearchIndexerClient(
    IAzureSearchIndexerClientFactory azureSearchIndexerClientFactory,
    IOptions<AzureSearchOptions> searchOptions,
    ILogger<SearchIndexerClient> logger
) : ISearchIndexerClient
{
    private const int IndexerAlreadyRunning = 409;
    private readonly string _indexerName = searchOptions.Value.IndexerName;

    public async Task RunIndexer(CancellationToken cancellationToken = default)
    {
        var client = azureSearchIndexerClientFactory.Create();
        try
        {
            var isIndexerRunning = await client.IsIndexerRunningAsync(
                _indexerName,
                cancellationToken
            );
            if (!isIndexerRunning)
            {
                await client.RunIndexerAsync(_indexerName, cancellationToken);
                logger.LogDebug("Indexer has been started.");
            }
            else
            {
                logger.LogDebug("Indexer is already running.");
            }
        }
        catch (Azure.RequestFailedException e) when (e.Status == IndexerAlreadyRunning)
        {
            logger.LogWarning("Can not start Indexer because it is already running.");
        }
    }

    public async Task<bool> IsIndexerRunning(
        string indexerName,
        CancellationToken cancellationToken = default
    )
    {
        var client = azureSearchIndexerClientFactory.Create();
        return await client.IsIndexerRunningAsync(indexerName, cancellationToken);
    }

    public async Task ResetIndexer(CancellationToken cancellationToken = default)
    {
        var client = azureSearchIndexerClientFactory.Create();
        await client.ResetIndexerAsync(_indexerName, cancellationToken);
    }

    public async Task<bool> IndexerExists(CancellationToken cancellationToken = default)
    {
        var client = azureSearchIndexerClientFactory.Create();
        return await client.IndexerExists(_indexerName, cancellationToken);
    }
}

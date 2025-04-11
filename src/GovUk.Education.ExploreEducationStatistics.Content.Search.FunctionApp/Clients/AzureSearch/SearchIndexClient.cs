using GovUk.Education.ExploreEducationStatistics.Content.Search.FunctionApp.Options;
using Microsoft.Extensions.Options;

namespace GovUk.Education.ExploreEducationStatistics.Content.Search.FunctionApp.Clients.AzureSearch;

/// <summary>
/// Client for calling the configured Azure Search Indexer
/// </summary>
public class SearchIndexClient(
    IAzureSearchIndexerClientFactory azureSearchIndexerClientFactory,
    IOptions<AzureSearchOptions> searchOptions)
    : ISearchIndexClient
{
    private readonly string _indexerName = searchOptions.Value.IndexerName;

    public async Task RunIndexer(CancellationToken cancellationToken = default)
    {
        var client = azureSearchIndexerClientFactory.Create();
        await client.RunIndexerAsync(_indexerName, cancellationToken);
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

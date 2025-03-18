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
    private readonly string _indexName = searchOptions.Value.IndexName;

    public async Task RunIndexer(CancellationToken cancellationToken = default)
    {
        var client = azureSearchIndexerClientFactory.Create();
        await client.RunIndexerAsync(_indexName, cancellationToken);
    }

    public async Task ResetIndexer(CancellationToken cancellationToken = default)
    {
        var client = azureSearchIndexerClientFactory.Create();
        await client.ResetIndexerAsync(_indexName, cancellationToken);
    }
    
    public async Task<bool> IndexerExists(CancellationToken cancellationToken = default)
    {
        var client = azureSearchIndexerClientFactory.Create();
        return await client.IndexerExists(_indexName, cancellationToken);
    }
}

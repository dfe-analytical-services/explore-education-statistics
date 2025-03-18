namespace GovUk.Education.ExploreEducationStatistics.Content.Search.FunctionApp.Clients.AzureSearch;

/// <summary>
/// A thin wrapper around Azure's SearchIndexerClient to allow it to be mocked
/// </summary>
/// <param name="azureSearchIndexerClient">A real SearchIndexerClient instance</param>
public class AzureSearchIndexerClientWrapper(Azure.Search.Documents.Indexes.SearchIndexerClient azureSearchIndexerClient) : IAzureSearchIndexerClient
{
    public async Task ResetIndexerAsync(string indexName, CancellationToken cancellationToken) => await azureSearchIndexerClient.ResetIndexerAsync(indexName, cancellationToken);

    public async Task RunIndexerAsync(string indexName, CancellationToken cancellationToken) => await azureSearchIndexerClient.RunIndexerAsync(indexName, cancellationToken);
    public async Task<bool> IndexerExists(string indexName, CancellationToken cancellationToken)
    {
        try
        {
            var response = await azureSearchIndexerClient.GetIndexerAsync(indexName, cancellationToken);
            return response.HasValue;
        }
        catch (Azure.RequestFailedException)
        {
            return false;
        }
    }
}

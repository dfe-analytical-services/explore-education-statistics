using Azure.Search.Documents.Indexes.Models;

namespace GovUk.Education.ExploreEducationStatistics.Content.Search.FunctionApp.Clients.AzureSearch;

/// <summary>
/// A thin wrapper around Azure's SearchIndexerClient to allow it to be mocked
/// </summary>
/// <param name="azureSearchIndexerClient">A real SearchIndexerClient instance</param>
public class AzureSearchIndexerClientWrapper(
    Azure.Search.Documents.Indexes.SearchIndexerClient azureSearchIndexerClient
) : IAzureSearchIndexerClient
{
    public async Task ResetIndexerAsync(string indexerName, CancellationToken cancellationToken) =>
        await azureSearchIndexerClient.ResetIndexerAsync(indexerName, cancellationToken);

    public async Task RunIndexerAsync(string indexerName, CancellationToken cancellationToken) =>
        await azureSearchIndexerClient.RunIndexerAsync(indexerName, cancellationToken);

    public async Task<bool> IsIndexerRunningAsync(
        string indexerName,
        CancellationToken cancellationToken
    )
    {
        var response = await azureSearchIndexerClient.GetIndexerStatusAsync(
            indexerName,
            cancellationToken
        );
        return response.HasValue
            && response.Value.LastResult.Status == IndexerExecutionStatus.InProgress;
    }

    public async Task<bool> IndexerExists(string indexerName, CancellationToken cancellationToken)
    {
        try
        {
            var response = await azureSearchIndexerClient.GetIndexerAsync(
                indexerName,
                cancellationToken
            );
            return response.HasValue;
        }
        catch (Azure.RequestFailedException)
        {
            return false;
        }
    }
}

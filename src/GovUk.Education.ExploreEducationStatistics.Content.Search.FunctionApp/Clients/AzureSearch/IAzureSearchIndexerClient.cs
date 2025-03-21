namespace GovUk.Education.ExploreEducationStatistics.Content.Search.FunctionApp.Clients.AzureSearch;

/// <summary>
/// Interface around Microsoft's Azure SearchIndexerClient
/// </summary>
public interface IAzureSearchIndexerClient
{
    Task ResetIndexerAsync(string indexName, CancellationToken cancellationToken);
    Task RunIndexerAsync(string indexName, CancellationToken cancellationToken);
    Task<bool> IndexerExists(string indexName, CancellationToken cancellationToken);
}

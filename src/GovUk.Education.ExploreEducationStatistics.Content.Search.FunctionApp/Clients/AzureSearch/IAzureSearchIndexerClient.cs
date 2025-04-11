namespace GovUk.Education.ExploreEducationStatistics.Content.Search.FunctionApp.Clients.AzureSearch;

/// <summary>
/// Interface around Microsoft's Azure SearchIndexerClient
/// </summary>
public interface IAzureSearchIndexerClient
{
    Task ResetIndexerAsync(string indexerName, CancellationToken cancellationToken);
    Task RunIndexerAsync(string indexerName, CancellationToken cancellationToken);
    Task<bool> IndexerExists(string indexerName, CancellationToken cancellationToken);
}

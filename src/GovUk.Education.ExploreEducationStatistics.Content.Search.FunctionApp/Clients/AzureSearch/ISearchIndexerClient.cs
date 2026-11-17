namespace GovUk.Education.ExploreEducationStatistics.Content.Search.FunctionApp.Clients.AzureSearch;

public interface ISearchIndexerClient
{
    Task RunIndexer(string indexerName, CancellationToken cancellationToken = default);
    Task<bool> IsIndexerRunning(string indexerName, CancellationToken cancellationToken = default);
    Task ResetIndexer(string indexerName, CancellationToken cancellationToken = default);
    Task<bool> IndexerExists(string indexerName, CancellationToken cancellationToken = default);
}

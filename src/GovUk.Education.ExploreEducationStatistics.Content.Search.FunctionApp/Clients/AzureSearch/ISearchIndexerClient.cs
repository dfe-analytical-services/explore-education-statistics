namespace GovUk.Education.ExploreEducationStatistics.Content.Search.FunctionApp.Clients.AzureSearch;

public interface ISearchIndexerClient
{
    Task RunIndexer(CancellationToken cancellationToken = default);
    Task<bool> IsIndexerRunning(string indexerName, CancellationToken cancellationToken = default);
    Task ResetIndexer(CancellationToken cancellationToken = default);
    Task<bool> IndexerExists(CancellationToken cancellationToken = default);
}

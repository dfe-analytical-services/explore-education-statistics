namespace GovUk.Education.ExploreEducationStatistics.Content.Search.FunctionApp.Clients.AzureSearch;

public interface IAzureSearchIndexerClientFactory
{
    IAzureSearchIndexerClient Create();
}
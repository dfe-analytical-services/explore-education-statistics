using Azure;
using Azure.Identity;
using GovUk.Education.ExploreEducationStatistics.Content.Search.FunctionApp.Options;
using Microsoft.Extensions.Options;
using AzureSearchIndexerClient = Azure.Search.Documents.Indexes.SearchIndexerClient;

namespace GovUk.Education.ExploreEducationStatistics.Content.Search.FunctionApp.Clients.AzureSearch;

/// <summary>
/// Factory creates the appropriate instance of SearchIndexerClient
/// (depending on whether an access key has been configured)
/// and wraps it in a AzureSearchIndexerClientWrapper
/// </summary>
public class AzureSearchIndexerClientFactory(IOptions<AzureSearchOptions> options)
    : IAzureSearchIndexerClientFactory
{
    private readonly string _searchServiceEndpoint = options.Value.SearchServiceEndpoint;
    private readonly string? _accessKey = options.Value.SearchServiceAccessKey;

    public IAzureSearchIndexerClient Create()
    {
        try
        {
            return new AzureSearchIndexerClientWrapper(
                string.IsNullOrEmpty(_accessKey)
                    ? new AzureSearchIndexerClient(
                        new Uri(_searchServiceEndpoint),
                        new DefaultAzureCredential()
                    )
                    : new AzureSearchIndexerClient(
                        new Uri(_searchServiceEndpoint),
                        new AzureKeyCredential(_accessKey)
                    )
            );
        }
        catch (UriFormatException)
        {
            throw new Exception(
                $"""Invalid Search Service Endpoint URL:"{_searchServiceEndpoint}" """
            );
        }
    }
}

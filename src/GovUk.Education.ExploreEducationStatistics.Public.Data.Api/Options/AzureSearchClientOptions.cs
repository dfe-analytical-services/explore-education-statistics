using Azure;

namespace GovUk.Education.ExploreEducationStatistics.Public.Data.Api.Options;

public class AzureSearchClientOptions
{
    public const string Section = "AzureSearchClient";

    /// <summary>
    /// The name of the Search Index.
    /// </summary>
    public required string IndexName { get; init; }

    /// <summary>
    /// The API key credential used to authenticate requests against the search service.
    /// </summary>
    public AzureKeyCredential? Credential { get; init; }

    /// <summary>
    /// The URI endpoint of the Search Service. This is likely to be similar to https://{search_service}.search.windows.net. The URI must use HTTPS.
    /// </summary>
    public required Uri Endpoint { get; init; }
}

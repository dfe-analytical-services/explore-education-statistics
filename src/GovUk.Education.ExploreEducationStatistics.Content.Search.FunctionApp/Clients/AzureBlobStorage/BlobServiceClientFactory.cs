using System.Data.Common;
using Azure.Core;
using Azure.Storage.Blobs;

namespace GovUk.Education.ExploreEducationStatistics.Content.Search.FunctionApp.Clients.AzureBlobStorage;

/// <summary>
/// Creates a <see cref="BlobServiceClient"/> from a storage connection string, connecting either with a key or with
/// the given managed identity credential.
///
/// A connection string with an account key or a SAS, or "UseDevelopmentStorage=true", connects with that key or SAS
/// as usual. A connection string with only an account name, e.g. "AccountName=myaccount;", connects with the
/// credential instead.
/// </summary>
public static class BlobServiceClientFactory
{
    private static readonly string[] CredentialKeys = ["AccountKey", "SharedAccessSignature", "UseDevelopmentStorage"];

    public static BlobServiceClient Create(
        string connectionString,
        TokenCredential credential,
        BlobClientOptions? options = null
    )
    {
        var settings = ParseConnectionString(connectionString);

        return CredentialKeys.Any(settings.ContainsKey)
            ? new BlobServiceClient(connectionString, options)
            : new BlobServiceClient(GetBlobServiceUri(settings), credential, options);
    }

    private static Uri GetBlobServiceUri(Dictionary<string, string> settings)
    {
        if (settings.TryGetValue("BlobEndpoint", out var endpoint))
        {
            return new Uri(endpoint);
        }

        if (!settings.TryGetValue("AccountName", out var accountName))
        {
            throw new ArgumentException("Storage connection string has neither an AccountName nor a BlobEndpoint");
        }

        var protocol = settings.GetValueOrDefault("DefaultEndpointsProtocol", "https");
        var endpointSuffix = settings.GetValueOrDefault("EndpointSuffix", "core.windows.net");

        return new Uri($"{protocol}://{accountName}.blob.{endpointSuffix}");
    }

    private static Dictionary<string, string> ParseConnectionString(string connectionString)
    {
        var builder = new DbConnectionStringBuilder { ConnectionString = connectionString };

        return builder
            .Keys.Cast<string>()
            .ToDictionary(key => key, key => (string)builder[key], StringComparer.OrdinalIgnoreCase);
    }
}

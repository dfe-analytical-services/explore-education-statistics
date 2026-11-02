using System.Data.Common;
using Azure.Core;
using Azure.Data.Tables;
using Azure.Identity;
using Azure.Storage.Blobs;
using Azure.Storage.Queues;

namespace GovUk.Education.ExploreEducationStatistics.Common.Utils;

/// <summary>
/// Creates Azure Storage clients from a storage connection string, connecting either with a key or with the app's
/// managed identity.
///
/// A connection string with an account key or a SAS, or "UseDevelopmentStorage=true", connects with that key or SAS
/// as usual. A connection string with only an account name, e.g. "AccountName=myaccount;", connects with the app's
/// managed identity instead: the user-assigned identity named by the "AZURE_CLIENT_ID" setting if there is one, or the
/// system-assigned identity otherwise.
/// </summary>
public static class StorageClientFactory
{
    private const string DefaultEndpointsProtocol = "https";
    private const string DefaultEndpointSuffix = "core.windows.net";

    private static readonly string[] CredentialKeys = ["AccountKey", "SharedAccessSignature", "UseDevelopmentStorage"];

    // Shared so that every client reuses the same cached tokens.
    private static readonly Lazy<TokenCredential> ManagedIdentityCredential = new(() =>
    {
        var clientId = Environment.GetEnvironmentVariable("AZURE_CLIENT_ID");
        return new DefaultAzureCredential(
            new DefaultAzureCredentialOptions
            {
                ManagedIdentityClientId = string.IsNullOrEmpty(clientId) ? null : clientId,
            }
        );
    });

    public static BlobServiceClient CreateBlobServiceClient(string connectionString, BlobClientOptions? options = null)
    {
        return UsesManagedIdentity(connectionString)
            ? new BlobServiceClient(GetServiceUri(connectionString, "blob"), ManagedIdentityCredential.Value, options)
            : new BlobServiceClient(connectionString, options);
    }

    public static QueueServiceClient CreateQueueServiceClient(
        string connectionString,
        QueueClientOptions? options = null
    )
    {
        return UsesManagedIdentity(connectionString)
            ? new QueueServiceClient(GetServiceUri(connectionString, "queue"), ManagedIdentityCredential.Value, options)
            : new QueueServiceClient(connectionString, options);
    }

    public static QueueClient CreateQueueClient(
        string connectionString,
        string queueName,
        QueueClientOptions? options = null
    )
    {
        return CreateQueueServiceClient(connectionString, options).GetQueueClient(queueName);
    }

    public static TableServiceClient CreateTableServiceClient(
        string connectionString,
        TableClientOptions? options = null
    )
    {
        return UsesManagedIdentity(connectionString)
            ? new TableServiceClient(GetServiceUri(connectionString, "table"), ManagedIdentityCredential.Value, options)
            : new TableServiceClient(connectionString, options);
    }

    /// <summary>
    /// Whether the connection string has no key or SAS, so connects with the app's managed identity.
    /// </summary>
    public static bool UsesManagedIdentity(string connectionString)
    {
        var settings = ParseConnectionString(connectionString);
        return !CredentialKeys.Any(settings.ContainsKey);
    }

    /// <summary>
    /// Gets the endpoint of a storage service ("blob", "queue" or "table") from a connection string, either from its
    /// explicit endpoint (e.g. "BlobEndpoint") or from its account name.
    /// </summary>
    public static Uri GetServiceUri(string connectionString, string service)
    {
        var settings = ParseConnectionString(connectionString);

        if (settings.TryGetValue($"{service}Endpoint", out var endpoint))
        {
            return new Uri(endpoint);
        }

        if (!settings.TryGetValue("AccountName", out var accountName))
        {
            throw new ArgumentException(
                $"Storage connection string has neither an AccountName nor a {service} endpoint",
                nameof(connectionString)
            );
        }

        var protocol = settings.GetValueOrDefault("DefaultEndpointsProtocol", DefaultEndpointsProtocol);
        var endpointSuffix = settings.GetValueOrDefault("EndpointSuffix", DefaultEndpointSuffix);

        return new Uri($"{protocol}://{accountName}.{service}.{endpointSuffix}");
    }

    private static Dictionary<string, string> ParseConnectionString(string connectionString)
    {
        var builder = new DbConnectionStringBuilder { ConnectionString = connectionString };

        return builder
            .Keys.Cast<string>()
            .ToDictionary(key => key, key => (string)builder[key], StringComparer.OrdinalIgnoreCase);
    }
}

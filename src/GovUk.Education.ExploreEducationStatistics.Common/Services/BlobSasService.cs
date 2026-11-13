#nullable enable
using System.Collections.Concurrent;
using Azure;
using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using Azure.Storage.Sas;
using GovUk.Education.ExploreEducationStatistics.Common.Model;
using GovUk.Education.ExploreEducationStatistics.Common.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace GovUk.Education.ExploreEducationStatistics.Common.Services;

public class BlobSasService(
    DateTimeProvider dateTimeProvider,
    ILogger<BlobSasService> logger,
    BlobSasService.ISecureBlobClientCreator? secureBlobClientCreator = null
) : IBlobSasService
{
    // How long each user delegation key is requested for, and so reused for. Azure allows up to 7 days.
    private static readonly TimeSpan UserDelegationKeyLifetime = TimeSpan.FromDays(1);

    private readonly ISecureBlobClientCreator _secureBlobClientCreator =
        secureBlobClientCreator ?? new SecureBlobClientCreator();

    // User delegation keys by storage account name, reused until they'd expire before the SAS they sign.
    private readonly ConcurrentDictionary<string, UserDelegationKey> _userDelegationKeys = new();

    public Task<Either<ActionResult, BlobDownloadToken>> CreateBlobDownloadToken(
        BlobServiceClient blobServiceClient,
        IBlobContainer container,
        string filename,
        string path,
        TimeSpan expiryDuration,
        CancellationToken cancellationToken
    )
    {
        var containerName = GetContainerName(blobServiceClient, container);

        return GetBlobClientOrNotFound(blobServiceClient: blobServiceClient, containerName: containerName, path: path)
            .OnSuccess(async blobClient =>
            {
                BlobProperties blobProperties = await blobClient.GetPropertiesAsync(
                    cancellationToken: cancellationToken
                );

                var uri = await CreateSasUrl(
                    blobServiceClient: blobServiceClient,
                    client: blobClient,
                    containerName: containerName,
                    expiryDuration: expiryDuration,
                    cancellationToken: cancellationToken
                );

                var requestParameters = uri.Query[1..];

                return new BlobDownloadToken(
                    Token: requestParameters,
                    ContainerName: containerName,
                    Path: path,
                    Filename: filename,
                    ContentType: blobProperties.ContentType
                );
            });
    }

    public async Task<Either<ActionResult, BlobClient>> CreateSecureBlobClient(
        BlobServiceClient blobServiceClient,
        BlobDownloadToken token
    )
    {
        return await GetBlobClientOrNotFound(
                blobServiceClient: blobServiceClient,
                containerName: token.ContainerName,
                path: token.Path
            )
            .OnSuccess(originalBlobClient =>
                _secureBlobClientCreator.CreateSecureBlobClient(
                    blobClientUri: originalBlobClient.Uri,
                    sasToken: token.Token
                )
            );
    }

    public Task<Uri> CreateContainerReadSasUri(
        BlobServiceClient blobServiceClient,
        BlobContainerClient containerClient,
        TimeSpan expiryDuration,
        CancellationToken cancellationToken = default
    )
    {
        return CreateContainerReadSasUri(
            blobServiceClient: blobServiceClient,
            getResourceUri: () => containerClient.Uri,
            containerName: containerClient.Name,
            canGenerateSasUri: containerClient.CanGenerateSasUri,
            generateSasUri: containerClient.GenerateSasUri,
            expiryDuration: expiryDuration,
            cancellationToken: cancellationToken
        );
    }

    private Task<Uri> CreateSasUrl(
        BlobServiceClient blobServiceClient,
        BlobClient client,
        string containerName,
        TimeSpan expiryDuration,
        CancellationToken cancellationToken
    )
    {
        return CreateContainerReadSasUri(
            blobServiceClient: blobServiceClient,
            getResourceUri: () => client.Uri,
            containerName: containerName,
            canGenerateSasUri: client.CanGenerateSasUri,
            generateSasUri: client.GenerateSasUri,
            expiryDuration: expiryDuration,
            cancellationToken: cancellationToken
        );
    }

    /// <summary>
    /// Creates a read-only SAS for a whole container, appended to the URI of the container or of one of its blobs. The
    /// URI is only needed when signing with a user delegation key.
    /// </summary>
    private async Task<Uri> CreateContainerReadSasUri(
        BlobServiceClient blobServiceClient,
        Func<Uri> getResourceUri,
        string containerName,
        bool canGenerateSasUri,
        Func<BlobSasBuilder, Uri> generateSasUri,
        TimeSpan expiryDuration,
        CancellationToken cancellationToken
    )
    {
        // Create a SAS token that's valid for a very short time.
        var sasBuilder = new BlobSasBuilder
        {
            BlobContainerName = containerName,
            Resource = "c",
            ExpiresOn = dateTimeProvider.UtcNow.Add(expiryDuration),
        };

        sasBuilder.SetPermissions(BlobContainerSasPermissions.Read);

        // A client authorised with the account key signs the SAS with that key.
        if (canGenerateSasUri)
        {
            return generateSasUri(sasBuilder);
        }

        // A client authorised with a managed identity has no account key, so signs the SAS with a user delegation key
        // obtained with that identity instead.
        var userDelegationKey = await GetUserDelegationKey(
            blobServiceClient: blobServiceClient,
            sasExpiresOn: sasBuilder.ExpiresOn,
            cancellationToken: cancellationToken
        );

        var uriBuilder = new BlobUriBuilder(getResourceUri())
        {
            Sas = sasBuilder.ToSasQueryParameters(userDelegationKey, blobServiceClient.AccountName),
        };

        return uriBuilder.ToUri();
    }

    private async Task<UserDelegationKey> GetUserDelegationKey(
        BlobServiceClient blobServiceClient,
        DateTimeOffset sasExpiresOn,
        CancellationToken cancellationToken
    )
    {
        // A SAS stops working when the key that signed it expires, so only reuse a key that outlasts the SAS.
        if (
            _userDelegationKeys.TryGetValue(blobServiceClient.AccountName, out var cachedKey)
            && cachedKey.SignedExpiresOn >= sasExpiresOn
        )
        {
            return cachedKey;
        }

        var keyExpiresOn = new DateTimeOffset(dateTimeProvider.UtcNow.Add(UserDelegationKeyLifetime));

        UserDelegationKey userDelegationKey = await blobServiceClient.GetUserDelegationKeyAsync(
            new BlobGetUserDelegationKeyOptions(keyExpiresOn > sasExpiresOn ? keyExpiresOn : sasExpiresOn),
            cancellationToken
        );

        _userDelegationKeys[blobServiceClient.AccountName] = userDelegationKey;

        return userDelegationKey;
    }

    private async Task<Either<ActionResult, BlobClient>> GetBlobClientOrNotFound(
        BlobServiceClient blobServiceClient,
        string containerName,
        string path
    )
    {
        var blobContainer = blobServiceClient.GetBlobContainerClient(containerName);
        var client = blobContainer.GetBlobClient(path);
        if (await client.ExistsAsync())
        {
            return client;
        }

        logger.LogWarning("Could not find blob {containerName}/{path}", containerName, path);
        return new NotFoundResult();
    }

    private static string GetContainerName(BlobServiceClient client, IBlobContainer container)
    {
        return IsDevelopmentStorageAccount(client) ? container.EmulatedName : container.Name;
    }

    private static bool IsDevelopmentStorageAccount(BlobServiceClient client)
    {
        return client.AccountName.ToLower() == "devstoreaccount1";
    }

    public interface ISecureBlobClientCreator
    {
        BlobClient CreateSecureBlobClient(Uri blobClientUri, string sasToken);
    }

    private class SecureBlobClientCreator : ISecureBlobClientCreator
    {
        public BlobClient CreateSecureBlobClient(Uri blobClientUri, string sasToken)
        {
            return new BlobClient(blobUri: blobClientUri, credential: new AzureSasCredential(sasToken));
        }
    }
}

#nullable enable
using System.Collections.Concurrent;
using System.Diagnostics.CodeAnalysis;
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

    // Locks by storage account name, so that concurrent requests needing a new key share one request for it.
    private readonly ConcurrentDictionary<string, SemaphoreSlim> _userDelegationKeyLocks = new();

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

                var uri = await CreateBlobReadSasUri(
                    blobServiceClient: blobServiceClient,
                    blobClient: blobClient,
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

    public async Task<Uri> CreateBlobReadSasUri(
        BlobServiceClient blobServiceClient,
        BlobClient blobClient,
        TimeSpan expiryDuration,
        CancellationToken cancellationToken = default
    )
    {
        // Create a read-only SAS token for just this blob that's valid for a very short time. It's scoped to the blob,
        // not the container, so that the token can't be used to read any other blob in the container.
        var sasBuilder = new BlobSasBuilder
        {
            BlobContainerName = blobClient.BlobContainerName,
            BlobName = blobClient.Name,
            Resource = "b",
            ExpiresOn = dateTimeProvider.UtcNow.Add(expiryDuration),
        };

        sasBuilder.SetPermissions(BlobSasPermissions.Read);

        // A client authorised with the account key signs the SAS with that key.
        if (blobClient.CanGenerateSasUri)
        {
            return blobClient.GenerateSasUri(sasBuilder);
        }

        // A client authorised with a managed identity has no account key, so signs the SAS with a user delegation key
        // obtained with that identity instead.
        var userDelegationKey = await GetUserDelegationKey(
            blobServiceClient: blobServiceClient,
            sasExpiresOn: sasBuilder.ExpiresOn,
            cancellationToken: cancellationToken
        );

        var uriBuilder = new BlobUriBuilder(blobClient.Uri)
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
        var accountName = blobServiceClient.AccountName;

        if (TryGetCachedUserDelegationKey(accountName, sasExpiresOn, out var cachedKey))
        {
            return cachedKey;
        }

        var keyLock = _userDelegationKeyLocks.GetOrAdd(accountName, _ => new SemaphoreSlim(1, 1));
        await keyLock.WaitAsync(cancellationToken);

        try
        {
            // Check again, as another request may have fetched a new key while this one waited for the lock.
            if (TryGetCachedUserDelegationKey(accountName, sasExpiresOn, out cachedKey))
            {
                return cachedKey;
            }

            var keyExpiresOn = new DateTimeOffset(dateTimeProvider.UtcNow.Add(UserDelegationKeyLifetime));

            UserDelegationKey userDelegationKey = await blobServiceClient.GetUserDelegationKeyAsync(
                new BlobGetUserDelegationKeyOptions(keyExpiresOn > sasExpiresOn ? keyExpiresOn : sasExpiresOn),
                cancellationToken
            );

            _userDelegationKeys[accountName] = userDelegationKey;

            return userDelegationKey;
        }
        finally
        {
            keyLock.Release();
        }
    }

    private bool TryGetCachedUserDelegationKey(
        string accountName,
        DateTimeOffset sasExpiresOn,
        [NotNullWhen(true)] out UserDelegationKey? cachedKey
    )
    {
        // A SAS stops working when the key that signed it expires, so only reuse a key that outlasts the SAS.
        return _userDelegationKeys.TryGetValue(accountName, out cachedKey) && cachedKey.SignedExpiresOn >= sasExpiresOn;
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

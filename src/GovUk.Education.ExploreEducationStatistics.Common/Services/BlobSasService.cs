#nullable enable
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
    private readonly ISecureBlobClientCreator _secureBlobClientCreator =
        secureBlobClientCreator ?? new SecureBlobClientCreator();

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

                var uri = CreateSasUrl(
                    client: blobClient,
                    containerName: containerName,
                    expiryDuration: expiryDuration
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

    private Uri CreateSasUrl(BlobClient client, string containerName, TimeSpan expiryDuration)
    {
        // Check if BlobContainerClient object has been authorized with Shared Key
        if (client.CanGenerateSasUri)
        {
            // Create a SAS token that's valid for a very short time.
            var sasBuilder = new BlobSasBuilder
            {
                BlobContainerName = containerName,
                Resource = "c",
                ExpiresOn = dateTimeProvider.UtcNow.Add(expiryDuration),
            };

            sasBuilder.SetPermissions(BlobContainerSasPermissions.Read);

            return client.GenerateSasUri(sasBuilder);
        }

        throw new InvalidOperationException("Could not generate SAS");
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

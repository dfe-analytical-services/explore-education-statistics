using System.IO.Compression;
using System.Net.Mime;
using System.Text.RegularExpressions;
using Azure;
using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using Azure.Storage.Blobs.Specialized;
using Azure.Storage.DataMovement;
using Azure.Storage.DataMovement.Blobs;
using GovUk.Education.ExploreEducationStatistics.Common.Extensions;
using GovUk.Education.ExploreEducationStatistics.Common.Model;
using GovUk.Education.ExploreEducationStatistics.Common.Services.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Common.Utils;
using GovUk.Education.ExploreEducationStatistics.Common.Utils.Interfaces;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using BlobInfo = GovUk.Education.ExploreEducationStatistics.Common.Model.BlobInfo;
using BlobProperties = Azure.Storage.Blobs.Models.BlobProperties;
using CopyStatus = Azure.Storage.Blobs.Models.CopyStatus;
using TransferManager = Azure.Storage.DataMovement.TransferManager;

namespace GovUk.Education.ExploreEducationStatistics.Common.Services;

/// <summary>
/// Service to perform low-level operations with a blob store
/// (this will most likely be Azure Blob Storage, but could be any vendor).
///
/// Other services should try to consume this if possible as we try to
/// abstract away as many implementation details as possible to avoid coupling
/// with the underlying client library. These can and do have major breaking
/// changes that make it difficult to upgrade when relying on lower-level
/// details e.g. Azure SDK v12 is an entirely new package compared to v11.
/// </summary>
public abstract partial class BlobStorageService(
    string defaultConnectionString,
    BlobServiceClient client,
    ILogger<IBlobStorageService> logger,
    IStorageInstanceCreationUtil storageInstanceCreationUtil,
    IBlobSasService blobSasService
) : IBlobStorageService
{
    private static readonly TimeSpan DownloadTokenExpiryDuration = TimeSpan.FromMinutes(5);

    protected BlobStorageService(
        string connectionString,
        ILogger<IBlobStorageService> logger,
        IBlobSasService blobSasService
    )
        : this(
            connectionString,
            new BlobServiceClient(connectionString),
            logger,
            new StorageInstanceCreationUtil(),
            blobSasService
        ) { }

    public async Task<List<string>> GetBlobs(
        IBlobContainer containerName,
        string? prefixFilter = null,
        CancellationToken cancellationToken = default
    )
    {
        var container = await GetBlobContainerClient(containerName);

        return string.IsNullOrWhiteSpace(prefixFilter)
            ? await container
                .GetBlobsAsync(cancellationToken: cancellationToken)
                .Select(blob => blob.Name)
                .ToListAsync(cancellationToken)
            : await container
                .GetBlobsAsync(cancellationToken: cancellationToken)
                .Where(blob =>
                {
                    var releaseIdFilePrefix = GuidPattern();
                    var blobKey = releaseIdFilePrefix.Replace(blob.Name, string.Empty);

                    return blobKey.StartsWith(prefixFilter);
                })
                .Select(blob => blob.Name)
                .ToListAsync(cancellationToken);
    }

    public async Task<bool> CheckBlobExists(IBlobContainer containerName, string path)
    {
        var blob = await GetBlobClient(containerName, path);
        return await blob.ExistsAsync();
    }

    public async Task<BlobInfo?> FindBlob(IBlobContainer containerName, string path)
    {
        var exists = await CheckBlobExists(containerName, path);

        if (!exists)
        {
            return null;
        }

        return await GetBlob(containerName, path);
    }

    public async Task DeleteBlobs(
        IBlobContainer containerName,
        string? directoryPath = null,
        IBlobStorageService.DeleteBlobsOptions? options = null
    )
    {
        if (!directoryPath.IsNullOrEmpty())
        {
            // Forcefully add a trailing slash to prevent deleting blobs whose names begin with that string
            directoryPath = directoryPath.AppendTrailingSlash();
        }

        var blobContainer = await GetBlobContainerClient(containerName);

        logger.LogInformation("Deleting blobs from {ContainerName}/{Path}", blobContainer.Name, directoryPath);

        string? continuationToken = null;

        do
        {
            var blobPages = blobContainer
                .GetBlobsAsync(new GetBlobsOptions { Prefix = directoryPath })
                .AsPages(continuationToken);

            var deleteTasks = new List<Task>();

            await foreach (Page<BlobItem> page in blobPages)
            {
                foreach (var blob in page.Values)
                {
                    if (blob == null)
                    {
                        break;
                    }

                    var excluded = options?.ExcludeRegex?.IsMatch(blob.Name) ?? false;
                    var included = options?.IncludeRegex?.IsMatch(blob.Name) ?? true;

                    if (excluded || !included)
                    {
                        logger.LogInformation("Ignoring blob {ContainerName}/{Path}", blobContainer.Name, blob.Name);
                        continue;
                    }

                    logger.LogInformation("Deleting blob {ContainerName}/{Path}", blobContainer.Name, blob.Name);

                    deleteTasks.Add(blobContainer.DeleteBlobIfExistsAsync(blob.Name));
                }

                continuationToken = page.ContinuationToken;
            }

            await Task.WhenAll(deleteTasks);
        } while (continuationToken != string.Empty);
    }

    public async Task DeleteBlob(IBlobContainer containerName, string path)
    {
        var blob = await GetBlobClient(containerName, path);

        logger.LogInformation("Deleting blob {ContainerName}/{Path}", containerName, path);

        await blob.DeleteIfExistsAsync();
    }

    public async Task UploadFile(IBlobContainer containerName, string path, IFormFile file)
    {
        var blob = await GetBlobClient(containerName, path);

        var tempFilePath = await UploadToTemporaryFile(file);

        logger.LogInformation("Uploading file to blob {ContainerName}/{Path}", containerName, path);

        await blob.UploadAsync(path: tempFilePath, httpHeaders: new BlobHttpHeaders { ContentType = file.ContentType });
    }

    public async Task<bool> CopyBlobs(
        IBlobContainer sourceContainerName,
        IBlobContainer destinationContainerName,
        List<string> blobNames,
        CancellationToken cancellationToken = default
    )
    {
        var sourceContainerClient = client.GetBlobContainerClient("sourceContainerName");
        var destinationContainerClient = client.GetBlobContainerClient("destinationContainerName");

        var blobsProvider = new BlobsStorageResourceProvider();
        var transferManager = new TransferManager(new TransferManagerOptions());

        var progress = new Progress<TransferProgress>(p =>
        {
            logger.LogInformation(
                "Transferred {BytesTransferred}. {CompletedCount} completed, {FailedCount} failed, {SkippedCount} skipped, {InProgressCount} in progress, {QueuedCount} queued.",
                p.BytesTransferred,
                p.CompletedCount,
                p.FailedCount,
                p.SkippedCount,
                p.InProgressCount,
                p.QueuedCount
            );
        });

        var transferOptions = new TransferOptions()
        {
            CreationMode = StorageResourceCreationMode.OverwriteIfExists,
            ProgressHandlerOptions = new() { ProgressHandler = progress, TrackBytesTransferred = true },
        };

        var tasks = blobNames.Select(async blobName =>
        {
            var source = await blobsProvider.FromBlobAsync(new Uri(sourceContainerClient.Uri, blobName));
            var destination = await blobsProvider.FromBlobAsync(new Uri(destinationContainerClient.Uri, blobName));

            var transfer = await transferManager.StartTransferAsync(
                source,
                destination,
                transferOptions,
                cancellationToken
            );
            await transfer.WaitForCompletionAsync();
        });

        await Task.WhenAll(tasks);

        return true;
    }

    public async Task<bool> MoveBlob(
        IBlobContainer sourceContainer,
        string sourcePath,
        string destinationPath,
        IBlobContainer? destinationContainer = null
    )
    {
        var sourceContainerClient = await GetBlobContainerClient(sourceContainer);
        var destinationContainerClient = destinationContainer is not null
            ? await GetBlobContainerClient(destinationContainer)
            : sourceContainerClient;

        var sourceBlob = sourceContainerClient.GetBlobClient(sourcePath);
        if (!await sourceBlob.ExistsAsync())
        {
            logger.LogWarning(
                "Source blob not found while moving blob. Source: '{Source}' Destination: '{Destination}'",
                sourcePath,
                destinationPath
            );
            return false;
        }

        var destinationBlob = destinationContainerClient.GetBlobClient(destinationPath);
        if (await destinationBlob.ExistsAsync())
        {
            logger.LogWarning(
                "Destination already exists while moving blob. Source: '{Source}' Destination: '{Destination}'",
                sourcePath,
                destinationPath
            );
            return false;
        }

        // Lease the source blob for the copy operation
        // to prevent another client from modifying it.
        var lease = sourceBlob.GetBlobLeaseClient();

        // Specifying -1 for the lease interval creates an infinite lease.
        await lease.AcquireAsync(TimeSpan.FromSeconds(-1));

        try
        {
            await destinationBlob.StartCopyFromUriAsync(sourceBlob.Uri);

            // Get the destination blob's properties and log the progress
            BlobProperties destinationProperties = await destinationBlob.GetPropertiesAsync();
            while (destinationProperties.CopyStatus == CopyStatus.Pending)
            {
                await Task.Delay(1000);
                logger.LogInformation("Copy progress: {Progress}", destinationProperties.CopyProgress);
                destinationProperties = await destinationBlob.GetPropertiesAsync();
            }

            if (destinationProperties.CopyStatus != CopyStatus.Success)
            {
                return false;
            }
        }
        finally
        {
            await lease.ReleaseAsync();
        }

        await sourceBlob.DeleteAsync();
        return true;
    }

    private static async Task<string> UploadToTemporaryFile(IFormFile file)
    {
        var path = Path.GetTempFileName();

        if (file.Length > 0)
        {
            await using var stream = new FileStream(path, FileMode.Create);
            await file.CopyToAsync(stream);
        }

        return path;
    }

    public async Task UploadStream(
        IBlobContainer containerName,
        string path,
        Stream sourceStream,
        string contentType,
        string? contentEncoding = null,
        CancellationToken cancellationToken = default
    )
    {
        var blob = await GetBlobClient(containerName, path);

        logger.LogInformation("Uploading text to blob {ContainerName}/{Path}", containerName, path);

        sourceStream.SeekToBeginning();

        var httpHeaders = new BlobHttpHeaders { ContentEncoding = contentEncoding, ContentType = contentType };

        var compress = contentEncoding != null;

        if (compress)
        {
            await using var blobStream = await blob.OpenWriteAsync(
                overwrite: true,
                options: new BlobOpenWriteOptions { HttpHeaders = httpHeaders },
                cancellationToken: cancellationToken
            );

            await using var compressionStream = CompressionUtils.GetCompressionStream(
                targetStream: blobStream,
                contentEncoding: contentEncoding!,
                compressionMode: CompressionMode.Compress
            );

            await sourceStream.CopyToAsync(compressionStream, cancellationToken);
        }
        else
        {
            await blob.UploadAsync(
                content: sourceStream,
                httpHeaders: httpHeaders,
                cancellationToken: cancellationToken
            );
        }
    }

    public async Task UploadAsJson<T>(
        IBlobContainer containerName,
        string path,
        T content,
        string? contentEncoding = null,
        JsonSerializerSettings? settings = null,
        CancellationToken cancellationToken = default
    )
    {
        await using var stream = new MemoryStream();
        await using var jsonWriter = new JsonTextWriter(new StreamWriter(stream, leaveOpen: true));
        JsonSerializer.CreateDefault(settings).Serialize(jsonWriter, content, typeof(T));
        await jsonWriter.FlushAsync(cancellationToken);

        await UploadStream(
            containerName: containerName,
            path: path,
            sourceStream: stream,
            contentEncoding: contentEncoding,
            contentType: MediaTypeNames.Application.Json,
            cancellationToken: cancellationToken
        );
    }

    public async Task<Either<ActionResult, Stream>> DownloadToStream(
        IBlobContainer containerName,
        string path,
        Stream targetStream,
        bool decompress = true,
        CancellationToken cancellationToken = default
    )
    {
        return await GetBlobClientOrNotFound(containerName, path)
            .OnSuccess(async blob =>
            {
                if (decompress)
                {
                    BlobProperties blobProperties = await blob.GetPropertiesAsync(cancellationToken: cancellationToken);

                    // Check the ContentEncoding property to determine if the blob
                    // is compressed and only decompress if necessary.
                    if (blobProperties.ContentEncoding.IsNullOrEmpty())
                    {
                        await blob.DownloadToAsync(targetStream, cancellationToken);
                        targetStream.SeekToBeginning();
                    }
                    else
                    {
                        var blobStream = await blob.OpenReadAsync(cancellationToken: cancellationToken);
                        await CompressionUtils.DecompressToStream(
                            stream: blobStream,
                            targetStream: targetStream,
                            contentEncoding: blobProperties.ContentEncoding,
                            cancellationToken: cancellationToken
                        );
                    }
                }
                else
                {
                    await blob.DownloadToAsync(targetStream, cancellationToken);
                    targetStream.SeekToBeginning();
                }

                return targetStream;
            });
    }

    public async Task<Either<ActionResult, Stream>> GetDownloadStream(
        IBlobContainer containerName,
        string path,
        bool decompress = true,
        CancellationToken cancellationToken = default
    )
    {
        return await GetBlobClientOrNotFound(containerName, path)
            .OnSuccess(async blob =>
            {
                if (decompress)
                {
                    BlobProperties blobProperties = await blob.GetPropertiesAsync(cancellationToken: cancellationToken);

                    // Check the ContentEncoding property to determine if the blob
                    // is compressed and only decompress if necessary.
                    if (blobProperties.ContentEncoding.IsNullOrEmpty())
                    {
                        return await blob.OpenReadAsync(cancellationToken: cancellationToken);
                    }

                    var blobStream = await blob.OpenReadAsync(cancellationToken: cancellationToken);
                    return CompressionUtils.GetCompressionStream(
                        blobStream,
                        contentEncoding: blobProperties.ContentEncoding,
                        CompressionMode.Decompress
                    );
                }

                return await blob.OpenReadAsync(cancellationToken: cancellationToken);
            });
    }

    private static async Task<Either<ActionResult, Stream>> GetDownloadStream(
        BlobClient blob,
        bool decompress = true,
        CancellationToken cancellationToken = default
    )
    {
        if (decompress)
        {
            BlobProperties blobProperties = await blob.GetPropertiesAsync(cancellationToken: cancellationToken);

            // Check the ContentEncoding property to determine if the blob
            // is compressed and only decompress if necessary.
            if (blobProperties.ContentEncoding.IsNullOrEmpty())
            {
                return await blob.OpenReadAsync(cancellationToken: cancellationToken);
            }

            var blobStream = await blob.OpenReadAsync(cancellationToken: cancellationToken);
            return CompressionUtils.GetCompressionStream(
                blobStream,
                contentEncoding: blobProperties.ContentEncoding,
                CompressionMode.Decompress
            );
        }

        return await blob.OpenReadAsync(cancellationToken: cancellationToken);
    }

    public Task<Either<ActionResult, FileStreamResult>> StreamWithToken(
        BlobDownloadToken token,
        CancellationToken cancellationToken
    )
    {
        return blobSasService
            .CreateSecureBlobClient(blobServiceClient: client, token: token)
            .OnSuccess(blobClient => GetDownloadStream(blob: blobClient, decompress: true, cancellationToken))
            .OnSuccess(stream => new FileStreamResult(fileStream: stream, contentType: token.ContentType)
            {
                FileDownloadName = token.Filename,
            });
    }

    public async Task<Either<ActionResult, BlobDownloadToken>> GetBlobDownloadToken(
        IBlobContainer container,
        string filename,
        string path,
        CancellationToken cancellationToken
    )
    {
        return await blobSasService.CreateBlobDownloadToken(
            blobServiceClient: client,
            container: container,
            filename: filename,
            path: path,
            expiryDuration: DownloadTokenExpiryDuration,
            cancellationToken: cancellationToken
        );
    }

    public async Task<Either<ActionResult, string>> DownloadBlobText(
        IBlobContainer containerName,
        string path,
        CancellationToken cancellationToken = default
    )
    {
        var blob = await GetBlobClient(containerName, path);

        try
        {
            BlobDownloadResult response = await blob.DownloadContentAsync(cancellationToken);

            // Check the ContentEncoding property to determine if the blob
            // is compressed and only decompress if necessary.
            var contentEncoding = response.Details.ContentEncoding;
            if (contentEncoding.IsNullOrEmpty())
            {
                return response.Content.ToString();
            }

            return await CompressionUtils.DecompressToString(
                bytes: response.Content.ToArray(),
                contentEncoding: contentEncoding,
                cancellationToken: cancellationToken
            );
        }
        catch (RequestFailedException exception) when (exception.Status == 404)
        {
            return new NotFoundResult();
        }
    }

    public async Task<Either<ActionResult, object?>> GetDeserializedJson(
        IBlobContainer containerName,
        string path,
        Type type,
        JsonSerializerSettings? settings = null,
        CancellationToken cancellationToken = default
    )
    {
        return await DownloadBlobText(containerName, path, cancellationToken)
            .OnSuccess(text =>
            {
                if (string.IsNullOrWhiteSpace(text))
                {
                    throw new JsonException($"Found empty file when trying to deserialize JSON for path: {path}");
                }

                return JsonConvert.DeserializeObject(value: text, type, settings);
            });
    }

    public async Task<Either<ActionResult, T?>> GetDeserializedJson<T>(
        IBlobContainer containerName,
        string path,
        JsonSerializerSettings? settings = null,
        CancellationToken cancellationToken = default
    )
        where T : class
    {
        return (await GetDeserializedJson(containerName, path, typeof(T), settings, cancellationToken)).OnSuccess(
            deserialized => deserialized as T
        );
    }

    public async Task<bool> CopyDirectory(
        IBlobContainer sourceContainerName,
        string sourceDirectoryPath,
        IBlobContainer destinationContainerName,
        string destinationDirectoryPath,
        CancellationToken cancellationToken = default
    )
    {
        logger.LogInformation(
            "Copying directory from {SourceContainer}/{SourcePath} to {DestinationContainer}/{DestinationPath}",
            sourceContainerName,
            sourceDirectoryPath,
            destinationContainerName,
            destinationDirectoryPath
        );

        var sourceContainerClient = client.GetBlobContainerClient("sourceContainerName");
        var destinationContainerClient = client.GetBlobContainerClient("destinationContainerName");

        var blobsProvider = new BlobsStorageResourceProvider();
        var transferManager = new TransferManager(new TransferManagerOptions());

        var progress = new Progress<TransferProgress>(p =>
        {
            logger.LogInformation(
                "Transferred {BytesTransferred}. {CompletedCount} completed, {FailedCount} failed, {SkippedCount} skipped, {InProgressCount} in progress, {QueuedCount} queued.",
                p.BytesTransferred,
                p.CompletedCount,
                p.FailedCount,
                p.SkippedCount,
                p.InProgressCount,
                p.QueuedCount
            );
        });

        var transferOperation = await transferManager.StartTransferAsync(
            sourceResource: await blobsProvider.FromContainerAsync(
                sourceContainerClient.Uri,
                new BlobStorageResourceContainerOptions() { BlobPrefix = sourceDirectoryPath },
                cancellationToken
            ),
            destinationResource: await blobsProvider.FromContainerAsync(
                destinationContainerClient.Uri,
                new BlobStorageResourceContainerOptions() { BlobPrefix = destinationDirectoryPath },
                cancellationToken
            ),
            transferOptions: new TransferOptions()
            {
                CreationMode = StorageResourceCreationMode.OverwriteIfExists,
                ProgressHandlerOptions = new() { ProgressHandler = progress, TrackBytesTransferred = true },
            },
            cancellationToken: cancellationToken
        );

        await transferOperation.WaitForCompletionAsync(cancellationToken);
        return transferOperation.Status.HasCompletedSuccessfully;
    }

    public async Task MoveDirectory(
        IBlobContainer sourceContainerName,
        string sourceDirectoryPath,
        IBlobContainer destinationContainerName,
        string destinationDirectoryPath,
        CancellationToken cancellationToken = default
    )
    {
        var copySuccessful = await CopyDirectory(
            sourceContainerName: sourceContainerName,
            sourceDirectoryPath: sourceDirectoryPath,
            destinationContainerName: destinationContainerName,
            destinationDirectoryPath: destinationDirectoryPath,
            cancellationToken
        );

        if (copySuccessful)
        {
            await DeleteBlobs(sourceContainerName, sourceDirectoryPath);
        }
    }

    private async Task<BlobClient> GetBlobClient(IBlobContainer containerName, string path)
    {
        var blobContainer = await GetBlobContainerClient(containerName);
        return blobContainer.GetBlobClient(path);
    }

    private async Task<Either<ActionResult, BlobClient>> GetBlobClientOrNotFound(
        IBlobContainer containerName,
        string path
    )
    {
        var blobClient = await GetBlobClient(containerName, path);
        if (await blobClient.ExistsAsync())
        {
            return blobClient;
        }

        logger.LogWarning("Could not find blob {ContainerName}/{Path}", containerName, path);
        return new NotFoundResult();
    }

    private async Task<BlobContainerClient> GetBlobContainerClient(IBlobContainer container)
    {
        var containerName = IsDevelopmentStorageAccount(client) ? container.EmulatedName : container.Name;

        var containerClient = client.GetBlobContainerClient(containerName);

        await storageInstanceCreationUtil.CreateInstanceIfNotExistsAsync(
            defaultConnectionString,
            AzureStorageType.Blob,
            containerName,
            () => containerClient.CreateIfNotExistsAsync()
        );

        return containerClient;
    }

    private static bool IsDevelopmentStorageAccount(BlobServiceClient client)
    {
        return client.AccountName.ToLower() == "devstoreaccount1";
    }

    private async Task<BlobInfo> GetBlob(IBlobContainer containerName, string path)
    {
        var blob = await GetBlobClient(containerName, path);
        BlobProperties properties = await blob.GetPropertiesAsync();

        return new BlobInfo(
            path: blob.Name,
            contentType: properties.ContentType,
            contentLength: properties.ContentLength,
            meta: properties.Metadata,
            created: properties.CreatedOn,
            updated: properties.LastModified
        );
    }

    [GeneratedRegex("[0-9a-fA-F]{8}-[0-9a-fA-F]{4}-[0-9a-fA-F]{4}-[0-9a-fA-F]{4}-[0-9a-fA-F]{12}/")]
    private static partial Regex GuidPattern();
}

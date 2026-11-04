using System.Collections.Concurrent;
using System.IO.Compression;
using System.Net.Mime;
using System.Text.RegularExpressions;
using Azure;
using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using Azure.Storage.Blobs.Specialized;
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
    private static readonly TimeSpan DefaultDownloadTokenExpiryDuration = TimeSpan.FromMinutes(5);

    // How long the source of a server-side copy can be read for. A copy within a region normally takes seconds.
    private static readonly TimeSpan CopySourceSasExpiryDuration = TimeSpan.FromHours(1);

    // How many blobs a directory copy copies at once.
    private const int MaxConcurrentDirectoryCopies = 8;

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
        var container = await GetBlobContainer(containerName);

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

        var blobContainer = await GetBlobContainer(containerName);

        logger.LogInformation("Deleting blobs from {containerName}/{path}", blobContainer.Name, directoryPath);

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
                        logger.LogInformation("Ignoring blob {containerName}/{path}", blobContainer.Name, blob.Name);
                        continue;
                    }

                    logger.LogInformation("Deleting blob {containerName}/{path}", blobContainer.Name, blob.Name);

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

        logger.LogInformation("Deleting blob {containerName}/{path}", containerName, path);

        await blob.DeleteIfExistsAsync();
    }

    public async Task UploadFile(IBlobContainer containerName, string path, IFormFile file)
    {
        var blob = await GetBlobClient(containerName, path);

        var tempFilePath = await UploadToTemporaryFile(file);

        logger.LogInformation("Uploading file to blob {containerName}/{path}", containerName, path);

        await blob.UploadAsync(path: tempFilePath, httpHeaders: new BlobHttpHeaders { ContentType = file.ContentType });
    }

    public async Task<bool> MoveBlob(
        IBlobContainer sourceContainer,
        string sourcePath,
        string destinationPath,
        IBlobContainer? destinationContainer = null
    )
    {
        var sourceContainerClient = await GetBlobContainer(sourceContainer);
        var destinationContainerClient = destinationContainer is not null
            ? await GetBlobContainer(destinationContainer)
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
            await destinationBlob.StartCopyFromUriAsync(
                await GetCopySourceUri(sourceContainerClient, sourcePath, CancellationToken.None)
            );

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

        logger.LogInformation("Uploading text to blob {containerName}/{path}", containerName, path);

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
        CancellationToken cancellationToken,
        TimeSpan? expiryDuration = null
    )
    {
        return await blobSasService.CreateBlobDownloadToken(
            blobServiceClient: client,
            container: container,
            filename: filename,
            path: path,
            expiryDuration: expiryDuration ?? DefaultDownloadTokenExpiryDuration,
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

    public async Task<List<BlobInfo>> CopyDirectory(
        IBlobContainer sourceContainerName,
        string sourceDirectoryPath,
        IBlobContainer destinationContainerName,
        string destinationDirectoryPath,
        IBlobStorageService.CopyDirectoryOptions? options = null
    )
    {
        logger.LogInformation(
            "Copying directory from {sourceContainer}/{sourcePath} to {destinationContainer}/{destinationPath}",
            sourceContainerName,
            sourceDirectoryPath,
            destinationContainerName,
            destinationDirectoryPath
        );

        var sourceContainer = await GetBlobContainer(sourceContainerName);
        var destinationContainer = options?.DestinationConnectionString is null
            ? await GetBlobContainer(destinationContainerName)
            : await GetBlobContainer(
                StorageClientFactory.CreateBlobServiceClient(options.DestinationConnectionString),
                options.DestinationConnectionString,
                destinationContainerName
            );

        var sourcePrefix = DirectoryPrefix(sourceDirectoryPath);
        var destinationPrefix = DirectoryPrefix(destinationDirectoryPath);

        var filesTransferred = new ConcurrentBag<BlobInfo>();

        await Parallel.ForEachAsync(
            sourceContainer.GetBlobsAsync(new GetBlobsOptions { Prefix = sourcePrefix, Traits = BlobTraits.Metadata }),
            new ParallelOptions { MaxDegreeOfParallelism = MaxConcurrentDirectoryCopies },
            async (sourceBlob, cancellationToken) =>
            {
                if (options?.ShouldCopyBlob?.Invoke(sourceBlob.Name) == false)
                {
                    return;
                }

                var destinationBlob = destinationContainer.GetBlobClient(
                    destinationPrefix + sourceBlob.Name[sourcePrefix.Length..]
                );

                var copied = await CopyBlobInDirectory(
                    sourceContainer: sourceContainer,
                    sourceBlobName: sourceBlob.Name,
                    destinationBlob: destinationBlob,
                    overwrite: options?.OverwriteExistingBlobs ?? false,
                    cancellationToken: cancellationToken
                );

                if (copied)
                {
                    filesTransferred.Add(
                        new BlobInfo(
                            path: destinationBlob.Name,
                            contentType: sourceBlob.Properties.ContentType,
                            contentLength: sourceBlob.Properties.ContentLength ?? 0,
                            meta: sourceBlob.Metadata,
                            created: sourceBlob.Properties.CreatedOn,
                            updated: sourceBlob.Properties.LastModified
                        )
                    );
                }
            }
        );

        return filesTransferred.ToList();
    }

    public async Task MoveDirectory(
        IBlobContainer sourceContainerName,
        string sourceDirectoryPath,
        IBlobContainer destinationContainerName,
        string destinationDirectoryPath,
        IBlobStorageService.MoveDirectoryOptions? options = null
    )
    {
        await CopyDirectory(
            sourceContainerName: sourceContainerName,
            sourceDirectoryPath: sourceDirectoryPath,
            destinationContainerName: destinationContainerName,
            destinationDirectoryPath: destinationDirectoryPath,
            options: new IBlobStorageService.CopyDirectoryOptions
            {
                DestinationConnectionString = options?.DestinationConnectionString,
                ShouldCopyBlob = options?.ShouldCopyBlob,
                OverwriteExistingBlobs = true,
            }
        );
        await DeleteBlobs(sourceContainerName, sourceDirectoryPath);
    }

    /// <summary>
    /// Copies one file during a directory copy. Azure Storage copies the data itself, so the file isn't downloaded
    /// and re-uploaded by this app. A failed or skipped copy is logged rather than thrown, so the rest of the directory
    /// is still copied.
    ///
    /// This method is use within a parallel operation so that multiple blobs can be copied at once.
    /// </summary>
    /// <returns>Whether the file was copied.</returns>
    private async Task<bool> CopyBlobInDirectory(
        BlobContainerClient sourceContainer,
        string sourceBlobName,
        BlobClient destinationBlob,
        bool overwrite,
        CancellationToken cancellationToken
    )
    {
        try
        {
            var sourceUri = await GetCopySourceUri(sourceContainer, sourceBlobName, cancellationToken);

            var copyOperation = await destinationBlob.StartCopyFromUriAsync(
                sourceUri,
                new BlobCopyFromUriOptions
                {
                    // Only copy if the destination doesn't exist yet, unless overwriting.
                    DestinationConditions = overwrite ? null : new BlobRequestConditions { IfNoneMatch = ETag.All },
                },
                cancellationToken
            );

            await copyOperation.WaitForCompletionAsync(cancellationToken);

            logger.LogInformation(
                "Transferred {sourceContainer}/{sourcePath} -> {destinationContainer}/{destinationPath}",
                sourceContainer.Name,
                sourceBlobName,
                destinationBlob.BlobContainerName,
                destinationBlob.Name
            );

            return true;
        }
        catch (RequestFailedException e) when (!overwrite && e.Status is 409 or 412)
        {
            logger.LogInformation(
                "Skipped transfer {sourceContainer}/{sourcePath} -> {destinationContainer}/{destinationPath}",
                sourceContainer.Name,
                sourceBlobName,
                destinationBlob.BlobContainerName,
                destinationBlob.Name
            );

            return false;
        }
        catch (Exception e) when (e is not OperationCanceledException)
        {
            logger.LogInformation(
                "Failed to transfer {sourceContainer}/{sourcePath} -> {destinationContainer}/{destinationPath}. Error message: {errorMessage}",
                sourceContainer.Name,
                sourceBlobName,
                destinationBlob.BlobContainerName,
                destinationBlob.Name,
                e.Message
            );

            return false;
        }
    }

    /// <summary>
    /// Gets a URI from which the destination of a server-side copy can read a source blob. It carries a short-lived,
    /// read-only SAS, as the destination may be in another storage account, or this client may connect with a managed
    /// identity, which a copy's source can't be authorised with directly.
    /// </summary>
    private async Task<Uri> GetCopySourceUri(
        BlobContainerClient sourceContainer,
        string sourceBlobName,
        CancellationToken cancellationToken
    )
    {
        var containerSasUri = await blobSasService.CreateContainerReadSasUri(
            blobServiceClient: client,
            containerClient: sourceContainer,
            expiryDuration: CopySourceSasExpiryDuration,
            cancellationToken: cancellationToken
        );

        return new BlobUriBuilder(containerSasUri) { BlobName = sourceBlobName }.ToUri();
    }

    /// <summary>
    /// Gets a directory path as a blob name prefix ending in "/", as it was treated by the previous copy library.
    /// </summary>
    private static string DirectoryPrefix(string directoryPath)
    {
        return directoryPath == string.Empty || directoryPath.EndsWith('/') ? directoryPath : $"{directoryPath}/";
    }

    private async Task<BlobClient> GetBlobClient(IBlobContainer containerName, string path)
    {
        var blobContainer = await GetBlobContainer(containerName);
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

        logger.LogWarning("Could not find blob {containerName}/{path}", containerName, path);
        return new NotFoundResult();
    }

    private Task<BlobContainerClient> GetBlobContainer(IBlobContainer container)
    {
        return GetBlobContainer(client, defaultConnectionString, container);
    }

    private async Task<BlobContainerClient> GetBlobContainer(
        BlobServiceClient serviceClient,
        string connectionString,
        IBlobContainer container
    )
    {
        var containerName = IsDevelopmentStorageAccount(serviceClient) ? container.EmulatedName : container.Name;

        var containerClient = serviceClient.GetBlobContainerClient(containerName);

        await storageInstanceCreationUtil.CreateInstanceIfNotExistsAsync(
            connectionString,
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

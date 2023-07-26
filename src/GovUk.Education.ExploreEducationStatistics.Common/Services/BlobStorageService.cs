#nullable enable
using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Mime;
using System.Threading;
using System.Threading.Tasks;
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
using Microsoft.Azure.Storage;
using Microsoft.Azure.Storage.Blob;
using Microsoft.Azure.Storage.DataMovement;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using BlobInfo = GovUk.Education.ExploreEducationStatistics.Common.Model.BlobInfo;
using BlobProperties = Azure.Storage.Blobs.Models.BlobProperties;
using CopyStatus = Azure.Storage.Blobs.Models.CopyStatus;

namespace GovUk.Education.ExploreEducationStatistics.Common.Services
{
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
    public abstract class BlobStorageService : IBlobStorageService
    {
        private readonly string _connectionString;
        private readonly BlobServiceClient _client;
        private readonly ILogger<IBlobStorageService> _logger;
        private readonly IStorageInstanceCreationUtil _storageInstanceCreationUtil;

        protected BlobStorageService(
            string connectionStringConfigName,
            ILogger<IBlobStorageService> logger,
            IConfiguration configuration)
        {
            var privateConnectionString = configuration.GetValue<string>(connectionStringConfigName);
            _connectionString = privateConnectionString;
            _client = new BlobServiceClient(privateConnectionString);
            _logger = logger;
            _storageInstanceCreationUtil = new StorageInstanceCreationUtil();
        }

        protected BlobStorageService(string connectionString, BlobServiceClient client, ILogger<IBlobStorageService> logger, IStorageInstanceCreationUtil storageInstanceCreationUtil)
        {
            _connectionString = connectionString;
            _client = client;
            _logger = logger;
            _storageInstanceCreationUtil = storageInstanceCreationUtil;
        }

        public async Task<List<BlobInfo>> ListBlobs(IBlobContainer containerName, string? path)
        {
            var blobContainer = await GetBlobContainer(containerName);
            var blobInfos = new List<BlobInfo>();

            string? continuationToken = null;

            do
            {
                var blobPages = blobContainer.GetBlobsAsync(BlobTraits.Metadata, prefix: path)
                    .AsPages(continuationToken);

                await foreach (Page<BlobItem> page in blobPages)
                {
                    foreach (var blob in page.Values)
                    {
                        if (blob == null)
                        {
                            break;
                        }

                        blobInfos.Add(
                            new BlobInfo(
                                path: blob.Name,
                                contentType: blob.Properties.ContentType,
                                contentLength: blob.Properties.ContentLength ?? 0,
                                meta: blob.Metadata,
                                created: blob.Properties.CreatedOn,
                                updated: blob.Properties.LastModified
                            )
                        );
                    }

                    continuationToken = page.ContinuationToken;
                }
            } while (continuationToken != string.Empty);

            return blobInfos;
        }

        public async Task<bool> CheckBlobExists(IBlobContainer containerName, string path)
        {
            var blob = await GetBlobClient(containerName, path);
            return await blob.ExistsAsync();
        }

        public async Task<BlobInfo> GetBlob(IBlobContainer containerName, string path)
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
            IBlobStorageService.DeleteBlobsOptions? options = null)
        {
            if (!directoryPath.IsNullOrEmpty())
            {
                // Forcefully add a trailing slash to prevent deleting blobs whose names begin with that string
                directoryPath = directoryPath?.AppendTrailingSlash();
            }

            var blobContainer = await GetBlobContainer(containerName);

            _logger.LogInformation("Deleting blobs from {containerName}/{path}", blobContainer.Name, directoryPath);

            string? continuationToken = null;

            do
            {
                var blobPages = blobContainer.GetBlobsAsync(prefix: directoryPath)
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
                            _logger.LogInformation("Ignoring blob {containerName}/{path}", blobContainer.Name, blob.Name);
                            continue;
                        }

                        _logger.LogInformation("Deleting blob {containerName}/{path}", blobContainer.Name, blob.Name);

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

            _logger.LogInformation("Deleting blob {containerName}/{path}", containerName, path);

            await blob.DeleteIfExistsAsync();
        }

        public async Task UploadFile(
            IBlobContainer containerName,
            string path,
            IFormFile file)
        {
            var blob = await GetBlobClient(containerName, path);

            var tempFilePath = await UploadToTemporaryFile(file);

            _logger.LogInformation("Uploading file to blob {containerName}/{path}", containerName, path);

            await blob.UploadAsync(
                path: tempFilePath,
                httpHeaders: new BlobHttpHeaders
                {
                    ContentType = file.ContentType
                }
            );
        }

        public async Task<bool> MoveBlob(IBlobContainer containerName,
            string sourcePath,
            string destinationPath)
        {
            var blobContainer = await GetBlobContainer(containerName);

            var destinationBlob = blobContainer.GetBlobClient(destinationPath);
            if (await destinationBlob.ExistsAsync())
            {
                _logger.LogWarning(
                    "Destination already exists while moving blob. Source: '{source}' Destination: '{destination}'",
                    sourcePath, destinationPath);
                return false;
            }

            var sourceBlob = blobContainer.GetBlobClient(sourcePath);
            if (!await sourceBlob.ExistsAsync())
            {
                _logger.LogWarning(
                    "Source blob not found while moving blob. Source: '{source}' Destination: '{destination}'",
                    sourcePath, destinationPath);
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
                    _logger.LogInformation("Copy progress: {progress}", destinationProperties.CopyProgress);
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
            Stream stream,
            string contentType,
            string? contentEncoding = null,
            CancellationToken cancellationToken = default)
        {
            var blob = await GetBlobClient(containerName, path);

            _logger.LogInformation("Uploading text to blob {containerName}/{path}", containerName, path);

            var httpHeaders = new BlobHttpHeaders
            {
                ContentEncoding = contentEncoding,
                ContentType = contentType
            };

            var compress = contentEncoding != null;
            if (compress)
            {
                await using var targetStream = new MemoryStream();
                await CompressionUtils.CompressToStream(
                    stream: stream,
                    targetStream: targetStream,
                    contentEncoding: contentEncoding!,
                    cancellationToken: cancellationToken);
                await blob.UploadAsync(
                    content: targetStream,
                    httpHeaders: httpHeaders,
                    cancellationToken: cancellationToken
                );
            }
            else
            {
                stream.SeekToBeginning();
                await blob.UploadAsync(
                    content: stream,
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
            CancellationToken cancellationToken = default)
        {
            await using var stream = new MemoryStream();
            await using var jsonWriter = new JsonTextWriter(new StreamWriter(stream, leaveOpen: true));
            JsonSerializer.CreateDefault(settings).Serialize(jsonWriter, content, typeof(T));
            await jsonWriter.FlushAsync(cancellationToken);

            await UploadStream(
                containerName: containerName,
                path: path,
                stream: stream,
                contentEncoding: contentEncoding,
                contentType: MediaTypeNames.Application.Json,
                cancellationToken: cancellationToken);
        }

        public async Task<Either<ActionResult, Stream>> DownloadToStream(
            IBlobContainer containerName,
            string path,
            Stream targetStream,
            bool decompress = true,
            CancellationToken cancellationToken = default)
        {
            return await GetBlobClientOrNotFound(containerName, path)
                .OnSuccess(async blob =>
                {
                    if (decompress)
                    {
                        BlobProperties blobProperties =
                            await blob.GetPropertiesAsync(cancellationToken: cancellationToken);

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
                                cancellationToken: cancellationToken);
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

        public async Task<Stream> StreamBlob(
            IBlobContainer containerName,
            string path,
            int? bufferSize = null,
            CancellationToken cancellationToken = default)
        {
            var blob = await GetBlobClient(containerName, path);

            try
            {
                return await blob.OpenReadAsync(bufferSize: bufferSize, cancellationToken: cancellationToken);
            }
            catch (RequestFailedException exception)
            {
                if (exception.Status == 404)
                {
                    ThrowFileNotFoundException(containerName, path);
                }

                throw;
            }
        }

        public async Task<Either<ActionResult, string>> DownloadBlobText(
            IBlobContainer containerName,
            string path,
            CancellationToken cancellationToken = default)
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
                    cancellationToken: cancellationToken);
            }
            catch (RequestFailedException exception)
                when (exception.Status == 404)
            {
                return new NotFoundResult();
            }
        }

        public async Task<Either<ActionResult, object?>> GetDeserializedJson(
            IBlobContainer containerName,
            string path,
            Type type,
            JsonSerializerSettings? settings = null,
            CancellationToken cancellationToken = default)
        {
            return await DownloadBlobText(containerName, path, cancellationToken)
                .OnSuccess(text =>
                {
                    if (string.IsNullOrWhiteSpace(text))
                    {
                        throw new JsonException(
                            $"Found empty file when trying to deserialize JSON for path: {path}");
                    }

                    return JsonConvert.DeserializeObject(
                        value: text,
                        type,
                        settings
                    );
                });
        }

        public async Task<Either<ActionResult, T?>> GetDeserializedJson<T>(
            IBlobContainer containerName,
            string path,
            JsonSerializerSettings? settings = null,
            CancellationToken cancellationToken = default)
            where T : class
        {
            return (await GetDeserializedJson(containerName, path, typeof(T), settings, cancellationToken))
                .OnSuccess(deserialized => deserialized as T);
        }

        public async Task<List<BlobInfo>> CopyDirectory(
            IBlobContainer sourceContainerName,
            string sourceDirectoryPath,
            IBlobContainer destinationContainerName,
            string destinationDirectoryPath,
            IBlobStorageService.CopyDirectoryOptions? options = null)
        {
            _logger.LogInformation(
                "Copying directory from {sourceContainer}/{sourcePath} to {destinationContainer}/{destinationPath}",
                sourceContainerName,
                sourceDirectoryPath,
                destinationContainerName,
                destinationDirectoryPath
            );

            var sourceContainer = await GetCloudBlobContainer(sourceContainerName);
            var destinationContainer = await GetCloudBlobContainer(
                destinationContainerName,
                connectionString: options?.DestinationConnectionString
            );

            var sourceDirectory = sourceContainer.GetDirectoryReference(sourceDirectoryPath);
            var destinationDirectory = destinationContainer.GetDirectoryReference(destinationDirectoryPath);

            var copyDirectoryOptions = new CopyDirectoryOptions
            {
                Recursive = true
            };

            var filesTransferred = new List<BlobInfo>();

            var context = new DirectoryTransferContext();
            context.FileTransferred += (sender, args) => FileTransferredCallback(sender, args, filesTransferred);
            context.FileFailed += FileFailedCallback;
            context.FileSkipped += FileSkippedCallback;
            context.SetAttributesCallbackAsync += options?.SetAttributesCallbackAsync;
            context.ShouldTransferCallbackAsync += options?.ShouldTransferCallbackAsync;
            context.ShouldOverwriteCallbackAsync += options?.ShouldOverwriteCallbackAsync;

            // TODO EES-4202 Find alternative to copying directory since TransferManager
            // depends on deprecated Microsoft.Azure.Storage.Blob SDK v11
            await TransferManager.CopyDirectoryAsync(
                sourceDirectory,
                destinationDirectory,
                CopyMethod.ServiceSideAsyncCopy,
                copyDirectoryOptions,
                context
            );

            return filesTransferred;
        }

        public async Task MoveDirectory(
            IBlobContainer sourceContainerName,
            string sourceDirectoryPath,
            IBlobContainer destinationContainerName,
            string destinationDirectoryPath,
            IBlobStorageService.MoveDirectoryOptions? options = null)
        {
            await CopyDirectory(
                sourceContainerName: sourceContainerName,
                sourceDirectoryPath: sourceDirectoryPath,
                destinationContainerName: destinationContainerName,
                destinationDirectoryPath: destinationDirectoryPath,
                options: new IBlobStorageService.CopyDirectoryOptions
                {
                    DestinationConnectionString = options?.DestinationConnectionString,
                    SetAttributesCallbackAsync = options?.SetAttributesCallbackAsync,
                    ShouldOverwriteCallbackAsync = (source, destination) => Task.FromResult(true),
                    ShouldTransferCallbackAsync = options?.ShouldTransferCallbackAsync,
                }
            );
            await DeleteBlobs(sourceContainerName, sourceDirectoryPath);
        }

        private void FileTransferredCallback(
            object sender,
            TransferEventArgs e,
            ICollection<BlobInfo> allFilesStream)
        {
            var source = (CloudBlockBlob) e.Source;
            var destination = (CloudBlockBlob) e.Destination;

            allFilesStream.Add(
                new BlobInfo(
                    path: destination.Name,
                    contentType: source.Properties.ContentType,
                    contentLength: source.Properties.Length,
                    meta: source.Metadata,
                    created: source.Properties.Created,
                    updated: source.Properties.LastModified
                )
            );

            _logger.LogInformation(
                "Transferred {sourceContainer}/{sourcePath} -> {destinationContainer}/{destinationPath}",
                source.Container.Name,
                source.Name,
                destination.Container.Name,
                destination.Name
            );
        }

        private void FileFailedCallback(object? sender, TransferEventArgs e)
        {
            var source = (CloudBlockBlob) e.Source;
            var destination = (CloudBlockBlob) e.Destination;

            _logger.LogInformation(
                "Failed to transfer {sourceContainer}/{sourcePath} -> {destinationContainer}/{destinationPath}. Error message: {errorMessage}",
                source.Container.Name,
                source.Name,
                destination.Container.Name,
                destination.Name,
                e.Exception.Message
            );
        }

        private void FileSkippedCallback(object? sender, TransferEventArgs e)
        {
            var source = (CloudBlockBlob) e.Source;
            var destination = (CloudBlockBlob) e.Destination;

            _logger.LogInformation(
                "Skipped transfer {sourceContainer}/{sourcePath} -> {destinationContainer}/{destinationPath}",
                source.Container.Name,
                source.Name,
                destination.Container.Name,
                destination.Name
            );
        }

        private async Task<BlobClient> GetBlobClient(IBlobContainer containerName, string path)
        {
            var blobContainer = await GetBlobContainer(containerName);
            return blobContainer.GetBlobClient(path);
        }

        private async Task<Either<ActionResult, BlobClient>> GetBlobClientOrNotFound(IBlobContainer containerName,
            string path)
        {
            var blobClient = await GetBlobClient(containerName, path);
            if (await blobClient.ExistsAsync())
            {
                return blobClient;
            }

            _logger.LogWarning("Could not find blob {containerName}/{path}", containerName, path);
            return new NotFoundResult();
        }

        /**
         * TODO EES-4202
         * We still need to use the older CloudBlobContainer implementation as we
         * need to interop with DataMovement.TransferManager which hasn't been
         * updated to work with Azure SDK 12 yet.
         */
        private async Task<CloudBlobContainer> GetCloudBlobContainer(
            IBlobContainer container,
            string? connectionString = null)
        {
            var storageAccount = CloudStorageAccount.Parse(connectionString ?? _connectionString);
            var blobClient = storageAccount.CreateCloudBlobClient();

            var containerName = IsDevelopmentStorageAccount(blobClient) ? container.EmulatedName : container.Name;

            var containerClient = blobClient.GetContainerReference(containerName);

            await _storageInstanceCreationUtil.CreateInstanceIfNotExistsAsync(
                _connectionString,
                AzureStorageType.Blob,
                containerName,
                () => containerClient.CreateIfNotExistsAsync());

            return containerClient;
        }

        private async Task<BlobContainerClient> GetBlobContainer(IBlobContainer container)
        {
            var containerName = IsDevelopmentStorageAccount(_client) ? container.EmulatedName : container.Name;

            var containerClient = _client.GetBlobContainerClient(containerName);

            await _storageInstanceCreationUtil.CreateInstanceIfNotExistsAsync(
                _connectionString,
                AzureStorageType.Blob,
                containerName,
                () => containerClient.CreateIfNotExistsAsync());

            return containerClient;
        }

        private static bool IsDevelopmentStorageAccount(BlobServiceClient client)
        {
            return client.AccountName.ToLower() == "devstoreaccount1";
        }

        private static bool IsDevelopmentStorageAccount(CloudBlobClient client)
        {
            return client.Credentials.AccountName.ToLower() == "devstoreaccount1";
        }

        private static void ThrowFileNotFoundException(IBlobContainer containerName, string path)
        {
            throw new FileNotFoundException($"Could not find file at {containerName}/{path}");
        }
    }
}

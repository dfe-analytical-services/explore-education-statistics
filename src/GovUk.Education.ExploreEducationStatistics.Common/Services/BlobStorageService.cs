using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Azure;
using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using Azure.Storage.Blobs.Specialized;
using GovUk.Education.ExploreEducationStatistics.Common.Extensions;
using GovUk.Education.ExploreEducationStatistics.Common.Services.Interfaces;
using Microsoft.AspNetCore.Http;
using Microsoft.Azure.Storage;
using Microsoft.Azure.Storage.Blob;
using Microsoft.Azure.Storage.DataMovement;
using Microsoft.Extensions.Logging;
using static GovUk.Education.ExploreEducationStatistics.Common.Services.FileStorageUtils;
using BlobInfo = GovUk.Education.ExploreEducationStatistics.Common.Model.BlobInfo;

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
    public class BlobStorageService : IBlobStorageService
    {
        private readonly string _connectionString;
        private readonly BlobServiceClient _client;
        private readonly ILogger<IBlobStorageService> _logger;

        public BlobStorageService(string connectionString, BlobServiceClient client, ILogger<IBlobStorageService> logger)
        {
            _connectionString = connectionString;
            _client = client;
            _logger = logger;
        }

        public async Task<IEnumerable<BlobInfo>> ListBlobs(string containerName, string path)
        {
            var blobContainer = await GetBlobContainer(containerName);
            var blobInfos = new List<BlobInfo>();

            string continuationToken = null;

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
                                size: GetSize(blob.Properties.ContentLength ?? 0),
                                contentType: blob.Properties.ContentType,
                                contentLength: blob.Properties.ContentLength ?? 0,
                                meta: blob.Metadata,
                                created: blob.Properties.CreatedOn
                            )
                        );
                    }

                    continuationToken = page.ContinuationToken;
                }
            } while (continuationToken != string.Empty);

            return blobInfos;
        }

        public async Task<bool> CheckBlobExists(string containerName, string path)
        {
            var blobContainer = await GetBlobContainer(containerName);
            return await blobContainer.GetBlobClient(path).ExistsAsync();
        }

        public async Task<BlobInfo> GetBlob(string containerName, string path)
        {
            var blobContainer = await GetBlobContainer(containerName);
            var blob = blobContainer.GetBlobClient(path);
            var properties = (await blob.GetPropertiesAsync()).Value;

            return new BlobInfo(
                path: blob.Name,
                size: GetSize(properties.ContentLength),
                contentType: properties.ContentType,
                contentLength: properties.ContentLength,
                meta: properties.Metadata,
                created: properties.CreatedOn
            );
        }

        public async Task DeleteBlobs(string containerName, string directoryPath, string excludePattern = null)
        {
            var prefix = directoryPath.AppendTrailingSlash();

            var blobContainer = await GetBlobContainer(containerName);

            _logger.LogInformation($"Deleting blobs from {blobContainer.Name}/{directoryPath}");

            string continuationToken = null;

            do
            {
                var blobPages = blobContainer.GetBlobsAsync(prefix: prefix)
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

                        var excluded = excludePattern != null &&
                                       Regex.IsMatch(blob.Name, excludePattern, RegexOptions.IgnoreCase);

                        if (excluded)
                        {
                            _logger.LogInformation($"Ignoring blob {blobContainer.Name}/{blob.Name}");
                            break;
                        }

                        _logger.LogInformation($"Deleting blob {blobContainer.Name}/{blob.Name}");

                        deleteTasks.Add(blobContainer.DeleteBlobIfExistsAsync(blob.Name));
                    }

                    continuationToken = page.ContinuationToken;
                }

                await Task.WhenAll(deleteTasks);
            } while (continuationToken != string.Empty);
        }

        public async Task DeleteBlob(string containerName, string path)
        {
            var blobContainer = await GetBlobContainer(containerName);
            var blob = blobContainer.GetBlobClient(path);

            _logger.LogInformation($"Deleting blob {containerName}/{path}");

            await blob.DeleteIfExistsAsync();
        }

        public async Task UploadFile(
            string containerName,
            string path,
            IFormFile file,
            IBlobStorageService.UploadFileOptions options = null)
        {
            var blobContainer = await GetBlobContainer(containerName);
            var blob = blobContainer.GetBlobClient(path);

            var tempFilePath = await UploadToTemporaryFile(file);

            _logger.LogInformation($"Uploading file to blob {containerName}/{path}");

            await blob.UploadAsync(
                tempFilePath,
                new BlobHttpHeaders
                {
                    ContentType = file.ContentType,
                },
                options?.MetaValues
            );
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
            string containerName,
            string path,
            Stream stream,
            string contentType,
            IBlobStorageService.UploadStreamOptions options = null)
        {
            var blobContainer = await GetBlobContainer(containerName);
            var blob = blobContainer.GetBlockBlobClient(path);

            _logger.LogInformation($"Uploading {containerName}/{path}");

            if (stream.CanSeek)
            {
                stream.Seek(0, SeekOrigin.Begin);
            }

            await blob.UploadAsync(
                stream,
                new BlobHttpHeaders
                {
                    ContentType = contentType,
                },
                options?.MetaValues
            );
        }

        public async Task UploadText(
            string containerName,
            string path,
            string content,
            string contentType,
            IBlobStorageService.UploadTextOptions options = null)
        {
            var blobContainer = await GetBlobContainer(containerName);
            var blob = blobContainer.GetBlockBlobClient(path);

            _logger.LogInformation($"Uploading text to blob {containerName}/{path}");

            await using var stream = new MemoryStream(Encoding.UTF8.GetBytes(content));

            await blob.UploadAsync(
                stream,
                new BlobHttpHeaders
                {
                    ContentType = contentType,
                },
                options?.MetaValues
            );
        }

        /**
         * Storage Emulator doesn't support AppendBlob. This method checks if AppendBlob can be used by either checking
         * for its presence or creating a new one.
         */
        public async Task<bool> IsAppendSupported(string containerName, string path)
        {
            var blobContainer = await GetBlobContainer(containerName);
            var blob = blobContainer.GetAppendBlobClient(path);

            if (await blob.ExistsAsync())
            {
                return true;
            }

            try
            {
                await blob.CreateIfNotExistsAsync();
                return true;
            }
            catch (StorageException e)
            {
                if (e.Message.Contains("Storage Emulator"))
                {
                    // Storage Emulator doesn't support AppendBlob
                    return false;
                }

                throw;
            }
        }


        public async Task AppendText(string containerName, string path, string content)
        {
            var blobContainer = await GetBlobContainer(containerName);
            var blob = blobContainer.GetAppendBlobClient(path);

            await using var stream = new MemoryStream(Encoding.UTF8.GetBytes(content));
            await blob.AppendBlockAsync(stream);
        }

        public async Task<Stream> DownloadToStream(string containerName, string path, Stream targetStream)
        {
            var blobContainer = await GetBlobContainer(containerName);
            var blob = blobContainer.GetAppendBlobClient(path);

            if (!await blob.ExistsAsync())
            {
                throw new FileNotFoundException($"Could not find file at {containerName}/{path}");
            }

            await blob.DownloadToAsync(targetStream);

            if (targetStream.CanSeek)
            {
                targetStream.Seek(0, SeekOrigin.Begin);
            }

            return targetStream;
        }

        public async Task<Stream> StreamBlob(string containerName, string path, int? bufferSize = null)
        {
            // Azure SDK v12 isn't compatible with how we want to use file
            // streams i.e. they need to be seekable. This is particularly
            // a problem for MIME type validation.
            // See: https://github.com/Azure/azure-sdk-for-net/pull/15032
            // TODO: Change to v12 implementation when possible
            var blobContainer = await GetCloudBlobContainer(containerName);
            var blob = blobContainer.GetBlockBlobReference(path);

            if (!await blob.ExistsAsync())
            {
                throw new FileNotFoundException($"Could not find file at {containerName}/{path}");
            }

            if (bufferSize != null)
            {
                blob.StreamMinimumReadSizeInBytes = (int) bufferSize;
            }

            return await blob.OpenReadAsync();
        }

        public async Task<string> DownloadBlobText(string containerName, string path)
        {
            var blobContainer = await GetBlobContainer(containerName);
            var blob = blobContainer.GetBlobClient(path);

            if (!await blob.ExistsAsync())
            {
                throw new FileNotFoundException($"Could not find file at {containerName}/{path}");
            }

            var properties = await blob.GetPropertiesAsync();
            if (properties.Value.ContentLength == 0)
            {
                return string.Empty;
            }

            await using var stream = await blob.OpenReadAsync();

            var streamReader = new StreamReader(stream);

            return await streamReader.ReadToEndAsync();
        }

        public async Task<List<BlobInfo>> CopyDirectory(
            string sourceContainerName,
            string sourceDirectoryPath,
            string destinationContainerName,
            string destinationDirectoryPath,
            IBlobStorageService.CopyDirectoryOptions options = null)
        {
            _logger.LogInformation(
                "Copying directory from {0}/{1} to {2}/{3}",
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
            string sourceContainerName,
            string sourceDirectoryPath,
            string destinationContainerName,
            string destinationDirectoryPath,
            IBlobStorageService.MoveDirectoryOptions options = null)
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
                    size: GetSize(source.Properties.Length),
                    contentType: source.Properties.ContentType,
                    contentLength: source.Properties.Length,
                    meta: source.Metadata,
                    created: source.Properties.Created
                )
            );

            _logger.LogInformation(
                "Transferred {0}/{1} -> {2}/{3}",
                source.Container.Name,
                source.Name,
                destination.Container.Name,
                destination.Name
            );
        }

        private void FileFailedCallback(object sender, TransferEventArgs e)
        {
            var source = (CloudBlockBlob) e.Source;
            var destination = (CloudBlockBlob) e.Destination;

            _logger.LogInformation(
                "Failed to transfer {0}/{1} -> {2}/{3}. Error message: {4}",
                source.Container.Name,
                source.Name,
                destination.Container.Name,
                destination.Name,
                e.Exception.Message
            );
        }

        private void FileSkippedCallback(object sender, TransferEventArgs e)
        {
            var source = (CloudBlockBlob) e.Source;
            var destination = (CloudBlockBlob) e.Destination;

            _logger.LogInformation(
                "Skipped transfer {0}/{1} -> {2}/{3}",
                source.Container.Name,
                source.Name,
                destination.Container.Name,
                destination.Name
            );
        }

        /**
         * We still need to use the older CloudBlobContainer implementation as we
         * need to interop with DataMovement.TransferManager which hasn't been
         * updated to work with Azure SDK 12 yet.
         */
        private async Task<CloudBlobContainer> GetCloudBlobContainer(
            string containerName,
            string connectionString = null)
        {
            var storageAccount = CloudStorageAccount.Parse(connectionString ?? _connectionString);
            var blobClient = storageAccount.CreateCloudBlobClient();

            var blobContainer = blobClient.GetContainerReference(containerName);
            await blobContainer.CreateIfNotExistsAsync();

            return blobContainer;
        }

        private async Task<BlobContainerClient> GetBlobContainer(string containerName)
        {
            var container = _client.GetBlobContainerClient(containerName);

            await container.CreateIfNotExistsAsync();

            return container;
        }
    }
}
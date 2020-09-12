using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using GovUk.Education.ExploreEducationStatistics.Common.Extensions;
using GovUk.Education.ExploreEducationStatistics.Common.Model;
using GovUk.Education.ExploreEducationStatistics.Common.Services.Interfaces;
using Microsoft.AspNetCore.Http;
using Microsoft.Azure.Storage;
using Microsoft.Azure.Storage.Blob;
using Microsoft.Azure.Storage.DataMovement;
using Microsoft.Extensions.Logging;
using static GovUk.Education.ExploreEducationStatistics.Common.Services.FileStorageUtils;

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
        private readonly string _storageConnectionString;
        private readonly ILogger<BlobStorageService> _logger;

        public BlobStorageService(string storageConnectionString, ILogger<BlobStorageService> logger)
        {
            _storageConnectionString =  storageConnectionString;
            _logger = logger;
        }

        public async Task<IEnumerable<BlobInfo>> ListBlobs(string containerName, string path)
        {
            var blobContainer = await GetCloudBlobContainer(containerName);

            return blobContainer
                .ListBlobs(path, true, BlobListingDetails.Metadata)
                .OfType<CloudBlockBlob>()
                .Select(
                    blob => new BlobInfo(
                        path: blob.Name,
                        size: GetSize(blob),
                        contentType: blob.Properties.ContentType,
                        length: blob.Properties.Length,
                        meta: blob.Metadata,
                        created: blob.Properties.Created
                    )
                );
        }

        public async Task<bool> CheckBlobExists(string containerName, string path)
        {
            var blobContainer = await GetCloudBlobContainer(containerName);
            var blob = blobContainer.GetBlockBlobReference(path);
           return blob.Exists();
        }

        public async Task<BlobInfo> GetBlob(string containerName, string path)
        {
            var blobContainer = await GetCloudBlobContainer(containerName);
            var blob = blobContainer.GetBlockBlobReference(path);

            await blob.FetchAttributesAsync();

            return new BlobInfo(
                path: blob.Name,
                size: GetSize(blob),
                contentType: blob.Properties.ContentType,
                length: blob.Properties.Length,
                meta: blob.Metadata,
                created: blob.Properties.Created
            );
        }

        public async Task DeleteBlobs(string containerName, string directoryPath, string excludePattern = null)
        {
            var container = await GetCloudBlobContainer(containerName);

            directoryPath = directoryPath.AppendTrailingSlash();
            _logger.LogInformation($"Deleting blobs from {container.Name} directory {directoryPath}");

            var token = new BlobContinuationToken();
            do
            {
                var result = await container.ListBlobsSegmentedAsync(
                    directoryPath,
                    true,
                    BlobListingDetails.None,
                    null,
                    token,
                    null,
                    null
                );

                token = result.ContinuationToken;

                var items = result.Results
                    .Select(item => item as CloudBlob)
                    .Where(
                        item =>
                        {
                            if (item == null)
                            {
                                return false;
                            }

                            var excluded = excludePattern != null &&
                                           Regex.IsMatch(item.Name, excludePattern, RegexOptions.IgnoreCase);
                            if (excluded)
                            {
                                _logger.LogInformation($"Ignoring {item.Name}");
                            }

                            return !excluded;
                        }
                    );

                await Task.WhenAll(
                    items
                        .Select(
                            item =>
                            {
                                _logger.LogInformation($"Deleting {item.Name}");
                                return item.DeleteIfExistsAsync();
                            }
                        )
                );
            } while (token != null);
        }

        public async Task DeleteBlob(string containerName, string fullPath)
        {
            var blobContainer = await GetCloudBlobContainer(containerName);
            await blobContainer.GetBlockBlobReference(fullPath).DeleteIfExistsAsync();
        }

        public async Task UploadFile(
            string containerName,
            string path,
            IFormFile file,
            IBlobStorageService.UploadFileOptions options = null)
        {
            var blobContainer = await GetCloudBlobContainer(containerName);
            var blob = blobContainer.GetBlockBlobReference(path);

            blob.Properties.ContentType = file.ContentType;

            if (options?.MetaValues != null)
            {
                AssignMetaValues(blob, options.MetaValues);
            }

            var tempFilePath = await UploadToTemporaryFile(file);
            await blob.UploadFromFileAsync(tempFilePath);
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
            var blobContainer = await GetCloudBlobContainer(containerName);
            var blob = blobContainer.GetBlockBlobReference(path);

            blob.Properties.ContentType = contentType;

            if (options?.MetaValues != null)
            {
                AssignMetaValues(blob, options.MetaValues);
            }

            await TransferManager.UploadAsync(stream, blob, new UploadOptions(), new SingleTransferContext());
        }

        public async Task UploadText(
            string containerName,
            string path,
            string content,
            string contentType,
            IBlobStorageService.UploadTextOptions options = null)
        {
            var blobContainer = await GetCloudBlobContainer(containerName);
            var blob = blobContainer.GetBlockBlobReference(path);

            blob.Properties.ContentType = contentType;

            if (options?.MetaValues != null)
            {
                AssignMetaValues(blob, options.MetaValues);
            }

            await blob.UploadTextAsync(content);
        }

        /**
         * Storage Emulator doesn't support AppendBlob. This method checks if AppendBlob can be used by either checking
         * for its presence or creating a new one.
         */
        public async Task<bool> IsAppendSupported(string containerName, string path)
        {
            var blobContainer = await GetCloudBlobContainer(containerName);
            var blob = blobContainer.GetAppendBlobReference(path);

            if (blob.Exists())
            {
                return true;
            }

            try
            {
                await blob.CreateOrReplaceAsync();
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


        public async Task AppendText(
            string containerName,
            string path,
            string content,
            string contentType = null,
            IBlobStorageService.AppendTextOptions options = null)
        {
            var blobContainer = await GetCloudBlobContainer(containerName);
            var blob = blobContainer.GetAppendBlobReference(path);

            blob.Properties.ContentType = contentType;

            if (options?.MetaValues != null)
            {
                AssignMetaValues(blob, options.MetaValues);
            }

            await blob.AppendTextAsync(content);
        }

        private static void AssignMetaValues(CloudBlob blob, IDictionary<string, string> metaValues)
        {
            foreach (var (key, value) in metaValues)
            {
                blob.Metadata[key] = value;
            }
        }

        public async Task<Stream> StreamBlob(string containerName, string path)
        {
            var blobContainer = await GetCloudBlobContainer(containerName);
            var blob = blobContainer.GetBlockBlobReference(path);

            if (!blob.Exists())
            {
                throw new FileNotFoundException($"Could not find file at {containerName}/{path}");
            }

            var stream = new MemoryStream();

            await blob.DownloadToStreamAsync(stream);
            stream.Seek(0, SeekOrigin.Begin);

            return stream;
        }

        public async Task<string> DownloadBlobText(string containerName, string path)
        {
            var blobContainer = await GetCloudBlobContainer(containerName);
            var blob = blobContainer.GetBlockBlobReference(path);

            if (!blob.Exists())
            {
                throw new FileNotFoundException($"Could not find file at {containerName}/{path}");
            }

            return await blob.DownloadTextAsync();
        }

        public async Task<List<BlobInfo>> CopyDirectory(
            string sourceContainerName,
            string sourceDirectoryPath,
            string destinationContainerName,
            string destinationDirectoryPath,
            IBlobStorageService.CopyDirectoryOptions options = null)
        {
            var sourceContainer = await GetCloudBlobContainer(sourceContainerName);
            var destinationContainer = options?.DestinationConnectionString != null
                ? await GetCloudBlobContainerAsync(options?.DestinationConnectionString, destinationContainerName)
                : await GetCloudBlobContainer(destinationContainerName);

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

            allFilesStream.Add(new BlobInfo(
                path: destination.Name,
                size: GetSize(source),
                contentType: source.Properties.ContentType,
                length: source.Properties.Length,
                meta: source.Metadata,
                created: source.Properties.Created
            ));

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
                "Failed to transfer {0}/{1} -> {2}. Error message: {3}",
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

        private async Task<CloudBlobContainer> GetCloudBlobContainer(
            string containerName,
            BlobContainerPermissions permissions = null,
            BlobRequestOptions requestOptions = null)
        {
            return await GetCloudBlobContainerAsync(
                _storageConnectionString,
                containerName,
                permissions,
                requestOptions
            );
        }
    }
}
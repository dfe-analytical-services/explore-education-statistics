using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Azure.Core;
using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Specialized;
using GovUk.Education.ExploreEducationStatistics.Common;
using GovUk.Education.ExploreEducationStatistics.Common.Model;
using GovUk.Education.ExploreEducationStatistics.Common.Services.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Content.Model;
using GovUk.Education.ExploreEducationStatistics.Content.Model.Extensions;
using GovUk.Education.ExploreEducationStatistics.Publisher.Models;
using GovUk.Education.ExploreEducationStatistics.Publisher.Services.Interfaces;
using ICSharpCode.SharpZipLib.Zip;
using Microsoft.Azure.Storage.Blob;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using static GovUk.Education.ExploreEducationStatistics.Common.Model.FileType;
using static GovUk.Education.ExploreEducationStatistics.Common.BlobContainers;
using static GovUk.Education.ExploreEducationStatistics.Common.Extensions.BlobInfoExtensions;
using static GovUk.Education.ExploreEducationStatistics.Common.Services.FileStoragePathUtils;
using static GovUk.Education.ExploreEducationStatistics.Common.Services.FileStorageUtils;
using File = GovUk.Education.ExploreEducationStatistics.Content.Model.File;

namespace GovUk.Education.ExploreEducationStatistics.Publisher.Services
{
    public class FileStorageService : IFileStorageService
    {
        private readonly IBlobStorageService _privateBlobStorageService;
        private readonly IBlobStorageService _publicBlobStorageService;
        private readonly string _publicStorageConnectionString;
        private readonly string _publisherStorageConnectionString;

        private readonly ILogger<FileStorageService> _logger;

        public FileStorageService(
            IBlobStorageService privateBlobStorageService,
            IBlobStorageService publicBlobStorageService,
            string publicStorageConnectionString,
            string publisherStorageConnectionString,
            ILogger<FileStorageService> logger)
        {
            _privateBlobStorageService = privateBlobStorageService;
            _publicBlobStorageService = publicBlobStorageService;
            _publicStorageConnectionString = publicStorageConnectionString;
            _publisherStorageConnectionString = publisherStorageConnectionString;
            _logger = logger;
        }

        public async Task<BlobLease> AcquireLease(string blobName)
        {
            // Ideally we should refactor BlobStorageService to do this, but it doesn't
            // really fit well with our interface. Additionally, we hope to completely scrap
            // table storage in favour of a database table, so we won't need this leasing
            // mechanism in the near future anyway.
            // TODO: Remove this in favour of database table for locking (EES-1232)
            var client = new BlobServiceClient(_publisherStorageConnectionString, new BlobClientOptions
            {
                Retry =
                {
                    Mode = RetryMode.Fixed,
                    Delay = TimeSpan.FromSeconds(5),
                    MaxRetries = 5
                }
            });

            var blobContainer = client.GetBlobContainerClient(PublisherLeases.Name);
            await blobContainer.CreateIfNotExistsAsync();

            var blob = blobContainer.GetBlobClient(blobName);
            var blobExists = await blob.ExistsAsync();

            if (!blobExists)
            {
                await using var stream = new MemoryStream(Encoding.UTF8.GetBytes(string.Empty));
                await blobContainer.UploadBlobAsync(blobName, stream);
            }

            var leaseClient = blob.GetBlobLeaseClient();
            await leaseClient.AcquireAsync(TimeSpan.FromSeconds(30));

            return new BlobLease(leaseClient);
        }

        public async Task<bool> CheckBlobExists(IBlobContainer containerName, string path)
        {
            return await _publicBlobStorageService.CheckBlobExists(containerName, path);
        }

        public async Task CopyReleaseFilesToPublicContainer(CopyReleaseFilesCommand copyReleaseFilesCommand)
        {
            var destinationDirectoryPath = $"{copyReleaseFilesCommand.ReleaseId}/";

            await _publicBlobStorageService.DeleteBlobs(
                containerName: PublicReleaseFiles,
                directoryPath: destinationDirectoryPath);

            var referencedRootPaths = copyReleaseFilesCommand.Files
                .Select(f => f.RootPath)
                .Distinct();

            foreach (var rootPath in referencedRootPaths)
            {
                await CopyPrivateFilesToPublic(
                    sourceDirectoryPath: $"{rootPath}/",
                    destinationDirectoryPath: destinationDirectoryPath,
                    copyReleaseFilesCommand
                );
            }

            var zipFiles = copyReleaseFilesCommand.Files
                .Where(file => file.Type != Chart && file.Type != Image)
                .ToList();

            await UploadZippedFiles(
                PublicReleaseFiles,
                destinationPath: PublicReleaseAllFilesZipPath(
                    copyReleaseFilesCommand.ReleaseId,
                    copyReleaseFilesCommand.PublicationSlug,
                    copyReleaseFilesCommand.ReleaseSlug),
                zipFileName: "All files",
                files: zipFiles,
                copyReleaseFilesCommand: copyReleaseFilesCommand
            );
        }

        public async Task DeleteDownloadFilesForPreviousVersion(Release release)
        {
            if (release.PreviousVersion != null)
            {
                var directoryPath = $"{release.PreviousVersion.Id}/";

                await _publicBlobStorageService.DeleteBlobs(
                    containerName: PublicReleaseFiles,
                    directoryPath: directoryPath);
            }
        }

        public async Task DeleteAllContentAsyncExcludingStaging()
        {
            var excludePattern = $"^{PublicContentStagingPath()}/.+$";
            await DeletePublicBlobs(string.Empty, excludePattern);
        }

        public async Task DeletePublicBlobs(string directoryPath, string excludePattern = null)
        {
            await _publicBlobStorageService.DeleteBlobs(PublicContentContainerName, directoryPath, excludePattern);
        }

        public async Task DeletePublicBlob(string path)
        {
            await _publicBlobStorageService.DeleteBlob(PublicContentContainerName, path);
        }

        public async Task<BlobInfo> GetBlob(IBlobContainer containerName, string path)
        {
            return await _publicBlobStorageService.GetBlob(containerName, path);
        }

        public async Task MovePublicDirectory(IBlobContainer containerName, string sourceDirectoryPath, string destinationDirectoryPath)
        {
            await _publicBlobStorageService.MoveDirectory(
                sourceContainerName: containerName,
                sourceDirectoryPath: sourceDirectoryPath,
                destinationContainerName: containerName,
                destinationDirectoryPath: destinationDirectoryPath
            );
        }

        private async Task CopyPrivateFilesToPublic(
            string sourceDirectoryPath,
            string destinationDirectoryPath,
            CopyReleaseFilesCommand copyReleaseFilesCommand)
        {
            await _privateBlobStorageService.CopyDirectory(
                sourceContainerName: PrivateReleaseFiles,
                sourceDirectoryPath: sourceDirectoryPath,
                destinationContainerName: PublicReleaseFiles,
                destinationDirectoryPath: destinationDirectoryPath,
                new IBlobStorageService.CopyDirectoryOptions
                {
                    DestinationConnectionString = _publicStorageConnectionString,
                    SetAttributesCallbackAsync = (destination) =>
                        SetAttributesCallback(destination, copyReleaseFilesCommand.PublishScheduled),
                    ShouldTransferCallbackAsync = (source, _) =>
                        ShouldTransferCallback(
                            source: source,
                            sourceContainerName: PrivateReleaseFiles,
                            files: copyReleaseFilesCommand.Files)
                }
            );
        }

#pragma warning disable 1998
        private static async Task SetAttributesCallback(object destination, DateTime releasePublished)
#pragma warning restore 1998
        {
            if (destination is CloudBlockBlob cloudBlockBlob)
            {
                cloudBlockBlob.Metadata[ReleaseDateTimeKey] = releasePublished.ToString("o", CultureInfo.InvariantCulture);
            }
        }

        private async Task<bool> ShouldTransferCallback(object source,
            IBlobContainer sourceContainerName,
            List<File> files)
        {
            if (!(source is CloudBlockBlob item))
            {
                return false;
            }

            if (files.Exists(file => file.Path() == item.Name))
            {
                return true;
            }

            _logger.LogInformation("Not transferring {0}/{1}", sourceContainerName, item.Name);
            return false;
        }

        private async Task UploadZippedFiles(
            IBlobContainer containerName,
            string destinationPath,
            string zipFileName,
            IEnumerable<File> files,
            CopyReleaseFilesCommand copyReleaseFilesCommand)
        {
            await using var memoryStream = new MemoryStream();
            await using var zipOutputStream = new ZipOutputStream(memoryStream);

            zipOutputStream.SetLevel(1);

            foreach (var file in files)
            {
                var zipEntry = new ZipEntry(file.Filename);
                zipOutputStream.PutNextEntry(zipEntry);

                await _publicBlobStorageService.DownloadToStream(
                    containerName: containerName,
                    path: file.MigratedPublicPath(copyReleaseFilesCommand.ReleaseId),
                    stream: zipOutputStream);
            }

            zipOutputStream.Finish();

            await _publicBlobStorageService.UploadStream(
                containerName,
                destinationPath,
                memoryStream,
                // Should this be MetaTypeNames.Application.Zip?
                contentType: "application/x-zip-compressed",
                metadata: GetAllFilesZipMetaValues(
                    name: zipFileName,
                    releaseDateTime: copyReleaseFilesCommand.PublishScheduled));
        }

        public async Task UploadAsJson(string filePath, object value, JsonSerializerSettings settings = null)
        {
            await _publicBlobStorageService.UploadAsJson(
                PublicContentContainerName,
                filePath,
                value,
                settings
            );
        }
    }
}

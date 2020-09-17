using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net.Mime;
using System.Text;
using System.Threading.Tasks;
using Azure.Core;
using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Specialized;
using GovUk.Education.ExploreEducationStatistics.Common.Extensions;
using GovUk.Education.ExploreEducationStatistics.Common.Model;
using GovUk.Education.ExploreEducationStatistics.Common.Services.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Content.Model;
using GovUk.Education.ExploreEducationStatistics.Publisher.Models;
using GovUk.Education.ExploreEducationStatistics.Publisher.Services.Interfaces;
using ICSharpCode.SharpZipLib.Zip;
using Microsoft.Azure.Storage.Blob;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using static GovUk.Education.ExploreEducationStatistics.Common.Model.ReleaseFileTypes;
using static GovUk.Education.ExploreEducationStatistics.Common.BlobContainerNames;
using static GovUk.Education.ExploreEducationStatistics.Common.Extensions.BlobInfoExtensions;
using static GovUk.Education.ExploreEducationStatistics.Common.Services.FileStoragePathUtils;
using static GovUk.Education.ExploreEducationStatistics.Common.Services.FileStorageUtils;
using FileInfo = GovUk.Education.ExploreEducationStatistics.Common.Model.FileInfo;

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

            var blobContainer = client.GetBlobContainerClient(PublisherLeasesContainerName);
            await blobContainer.CreateIfNotExistsAsync();

            await using var stream = new MemoryStream(Encoding.UTF8.GetBytes(string.Empty));
            await blobContainer.UploadBlobAsync(blobName, stream);

            var leaseClient = blobContainer.GetBlobClient(blobName).GetBlobLeaseClient();
            await leaseClient.AcquireAsync(TimeSpan.FromSeconds(30));

            return new BlobLease(leaseClient);
        }

        public async Task CopyReleaseFilesToPublicContainer(CopyReleaseFilesCommand copyReleaseFilesCommand)
        {
            var destinationDirectoryPath =
                PublicReleaseDirectoryPath(copyReleaseFilesCommand.PublicationSlug, copyReleaseFilesCommand.ReleaseSlug);

            await _publicBlobStorageService.DeleteBlobs(PublicFilesContainerName, destinationDirectoryPath);

            var referencedReleaseVersions = copyReleaseFilesCommand.ReleaseFileReferences
                .Select(rfr => rfr.ReleaseId)
                .Distinct();

            var transferredFiles = new List<BlobInfo>();

            foreach (var version in referencedReleaseVersions)
            {
                var files = await CopyPrivateFilesToPublic(
                    sourceDirectoryPath: AdminReleaseDirectoryPath(version),
                    destinationDirectoryPath: destinationDirectoryPath,
                    copyReleaseFilesCommand
                );

                transferredFiles.AddRange(files);
            }

            var chartDirectory = PublicReleaseDirectoryPath(
                copyReleaseFilesCommand.PublicationSlug,
                copyReleaseFilesCommand.ReleaseSlug,
                Chart
            );

            var zipFiles = transferredFiles
                .Where(blob => !blob.Path.StartsWith(chartDirectory))
                .ToList();

            await UploadZippedFiles(
                PublicFilesContainerName,
                destinationPath: PublicReleasePath(
                        copyReleaseFilesCommand.PublicationSlug,
                        copyReleaseFilesCommand.ReleaseSlug,
                        ReleaseFileTypes.Ancillary,
                    $"{copyReleaseFilesCommand.PublicationSlug}_{copyReleaseFilesCommand.ReleaseSlug}.zip"
                    ),
                zipFileName: "All files",
                files: zipFiles,
                copyReleaseFilesCommand: copyReleaseFilesCommand
            );
        }

        public async Task DeleteDownloadFilesForPreviousVersion(Release release)
        {
            if (release.PreviousVersion != null && release.Slug != release.PreviousVersion.Slug)
            {
                var directoryPath = PublicReleaseDirectoryPath(release.Publication.Slug, release.PreviousVersion.Slug);

                await _publicBlobStorageService.DeleteBlobs(PublicFilesContainerName, directoryPath);
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

        public async Task<IEnumerable<FileInfo>> ListPublicFiles(string publication, string release)
        {
            var files = new List<BlobInfo>();

            files.AddRange(
                await _publicBlobStorageService.ListBlobs(
                    PublicFilesContainerName,
                    PublicReleaseDirectoryPath(publication, release, ReleaseFileTypes.Data)
                )
            );
            files.AddRange(
                await _publicBlobStorageService.ListBlobs(
                    PublicFilesContainerName,
                    PublicReleaseDirectoryPath(publication, release, ReleaseFileTypes.Ancillary)
                )
            );

            return files
                .Where(blob => !blob.IsMetaDataFile() && blob.IsReleased())
                .Select(blob => blob.ToFileInfo())
                .OrderBy(f => f.Name)
                .AsEnumerable();
        }

        public async Task MovePublicDirectory(string containerName, string sourceDirectoryPath, string destinationDirectoryPath)
        {
            await _publicBlobStorageService.MoveDirectory(
                sourceContainerName: containerName,
                sourceDirectoryPath: sourceDirectoryPath,
                destinationContainerName: containerName,
                destinationDirectoryPath: destinationDirectoryPath
            );
        }

        private async Task<List<BlobInfo>> CopyPrivateFilesToPublic(
            string sourceDirectoryPath,
            string destinationDirectoryPath,
            CopyReleaseFilesCommand copyReleaseFilesCommand)
        {
            return await _privateBlobStorageService.CopyDirectory(
                sourceContainerName: PrivateFilesContainerName,
                sourceDirectoryPath: sourceDirectoryPath,
                destinationContainerName: PublicFilesContainerName,
                destinationDirectoryPath: destinationDirectoryPath,
                new IBlobStorageService.CopyDirectoryOptions
                {
                    DestinationConnectionString = _publicStorageConnectionString,
                    SetAttributesCallbackAsync = (destination) =>
                        SetAttributesCallbackAsync(destination, copyReleaseFilesCommand.PublishScheduled),
                    ShouldTransferCallbackAsync = (source, _) =>
                        CopyFileUnlessBatchedOrMeta(
                            source,
                            copyReleaseFilesCommand.ReleaseId,
                            copyReleaseFilesCommand.ReleaseFileReferences),
                }
            );
        }

#pragma warning disable 1998
        private static async Task SetAttributesCallbackAsync(object destination, DateTime releasePublished)
#pragma warning restore 1998
        {
            if (destination is CloudBlockBlob cloudBlockBlob)
            {
                cloudBlockBlob.Metadata[ReleaseDateTimeKey] = releasePublished.ToString("o", CultureInfo.InvariantCulture);
            }
        }

        private async Task<bool> CopyFileUnlessBatchedOrMeta(object source, Guid releaseId, List<ReleaseFileReference> releaseFileReferences)
        {
            var item = source as CloudBlockBlob;
            if (item == null)
            {
                return false;
            }

            await item.FetchAttributesAsync();

            if (IsBatchedDataFile(item, releaseId) || IsMetaDataFile(item))
            {
                return false;
            }

            var name = Path.GetFileName(item.Name);

            if (releaseFileReferences.Exists(rfr => rfr.ReleaseId == releaseId && rfr.BlobStorageName == name))
            {
                return true;
            }

            _logger.LogError($"No release file reference found for releaseId {releaseId} and name: {name}");
            return false;

        }

        private async Task UploadZippedFiles(
            string containerName,
            string destinationPath,
            string zipFileName,
            IEnumerable<BlobInfo> files,
            CopyReleaseFilesCommand copyReleaseFilesCommand)
        {
            await using var memoryStream = new MemoryStream();
            await using var zipOutputStream = new ZipOutputStream(memoryStream);

            zipOutputStream.SetLevel(1);

            foreach (var file in files)
            {
                var zipEntry = new ZipEntry(file.FileName);
                zipOutputStream.PutNextEntry(zipEntry);

                await _publicBlobStorageService.DownloadToStream(containerName, file.Path, zipOutputStream);
            }

            zipOutputStream.Finish();

            await _publicBlobStorageService.UploadStream(
                containerName,
                destinationPath,
                memoryStream,
                // Should this be MetaTypeNames.Application.Zip?
                contentType: "application/x-zip-compressed",
                options: new IBlobStorageService.UploadStreamOptions
                {
                    MetaValues = new Dictionary<string, string>
                    {
                        {NameKey, zipFileName},
                        {
                            ReleaseDateTimeKey,
                            copyReleaseFilesCommand.PublishScheduled.ToString("o", CultureInfo.InvariantCulture)
                        }
                    }
                }
            );
        }

        public async Task UploadAsJson(string filePath, object value, JsonSerializerSettings settings = null)
        {
            var json = JsonConvert.SerializeObject(value, null, settings);

            await _publicBlobStorageService.UploadText(
                PublicContentContainerName,
                path: filePath,
                content: json,
                contentType: MediaTypeNames.Application.Json
            );
        }
    }
}
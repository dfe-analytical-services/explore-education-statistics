using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using GovUk.Education.ExploreEducationStatistics.Common.Extensions;
using GovUk.Education.ExploreEducationStatistics.Common.Model;
using GovUk.Education.ExploreEducationStatistics.Common.Services;
using GovUk.Education.ExploreEducationStatistics.Content.Model;
using GovUk.Education.ExploreEducationStatistics.Content.Model.Database;
using GovUk.Education.ExploreEducationStatistics.Publisher.Models;
using GovUk.Education.ExploreEducationStatistics.Publisher.Services.Interfaces;
using ICSharpCode.SharpZipLib.Zip;
using Microsoft.Azure.Storage.Blob;
using Microsoft.Azure.Storage.DataMovement;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using static GovUk.Education.ExploreEducationStatistics.Common.Model.ReleaseFileTypes;
using static GovUk.Education.ExploreEducationStatistics.Common.Services.FileStoragePathUtils;
using FileInfo = GovUk.Education.ExploreEducationStatistics.Common.Model.FileInfo;
using Task = System.Threading.Tasks.Task;
using static GovUk.Education.ExploreEducationStatistics.Common.Services.FileStorageUtils;

namespace GovUk.Education.ExploreEducationStatistics.Publisher.Services
{
    public class FileStorageService : IFileStorageService
    {
        private readonly ILogger<FileStorageService> _logger;

        private readonly string _privateStorageConnectionString;
        private readonly string _publicStorageConnectionString;
        private readonly ContentDbContext _contentDbContext;

        private const string PrivateFilesContainerName = "releases";
        private const string PublicFilesContainerName = "downloads";
        private const string PublicContentContainerName = "cache";

        public FileStorageService(
            IConfiguration configuration,
            ContentDbContext contentDbContext,
            ILogger<FileStorageService> logger)
        {
            _privateStorageConnectionString = configuration.GetValue<string>("CoreStorage");
            _publicStorageConnectionString = configuration.GetValue<string>("PublicStorage");
            _contentDbContext = contentDbContext;
            _logger = logger;
        }

        public async Task CopyReleaseToPublicContainer(CopyReleaseCommand copyReleaseCommand)
        {
            var privateContainer =
                await GetCloudBlobContainerAsync(_privateStorageConnectionString,
                    PrivateFilesContainerName);
            var publicContainer =
                await GetCloudBlobContainerAsync(_publicStorageConnectionString,
                    PublicFilesContainerName);

            // Slug may have changed for the amendment so also remove the previous contents if it has
            if (copyReleaseCommand.ReleaseSlug != copyReleaseCommand.PreviousVersionSlug)
            {
                var previousDestinationDirectoryPath =
                    PublicReleaseDirectoryPath(copyReleaseCommand.PublicationSlug,
                        copyReleaseCommand.PreviousVersionSlug);
                await DeleteBlobsAsync(publicContainer, previousDestinationDirectoryPath);
            }
            
            var destinationDirectoryPath =
                PublicReleaseDirectoryPath(copyReleaseCommand.PublicationSlug, copyReleaseCommand.ReleaseSlug);

            await DeleteBlobsAsync(publicContainer, destinationDirectoryPath);

            var releaseFileReferences = GetAllFilesUploaded(copyReleaseCommand.ReleaseId, ReleaseFileTypes.Data, Ancillary, Chart)
                .Select(rf => rf.ReleaseFileReference).ToList();
            
            var versions = releaseFileReferences.GroupBy(rfr => rfr.ReleaseId)
                .Select(grp => grp.First())
                .ToList();
            
            var allFilesTransferred = new List<CloudBlockBlob>();

            foreach (var version in versions)
            {
                var files = await CopyDirectoryAsync(
                    AdminReleaseDirectoryPath(version.ReleaseId), 
                    destinationDirectoryPath, privateContainer,
                    publicContainer, copyReleaseCommand,
                    (source, destination) =>
                        CopyFileUnlessBatchedOrMeta(
                            source,
                            version.ReleaseId,
                            releaseFileReferences));
                
                allFilesTransferred.AddRange(files);
            }

            await ZipFiles(publicContainer, allFilesTransferred, destinationDirectoryPath, copyReleaseCommand);
        }

        public async Task DeleteAllContentAsyncExcludingStaging()
        {
            var publicContainer = await FileStorageUtils.GetCloudBlobContainerAsync(_publicStorageConnectionString,
                PublicContentContainerName);
            var excludePattern = $"^{PublicContentStagingPath()}/.+$";
            await DeleteBlobsAsync(publicContainer, string.Empty, excludePattern);
        }

        public IEnumerable<FileInfo> ListPublicFiles(string publication, string release)
        {
            return FileStorageUtils.ListPublicFiles(_publicStorageConnectionString, PublicFilesContainerName,
                publication, release);
        }

        public async Task MoveStagedContentAsync()
        {
            var container = await FileStorageUtils.GetCloudBlobContainerAsync(_publicStorageConnectionString,
                PublicContentContainerName);

            var sourceDirectoryPath = PublicContentStagingPath();
            var sourceDirectory = container.GetDirectoryReference(sourceDirectoryPath);
            var destinationDirectory = container.GetDirectoryReference(string.Empty);

            var options = new CopyDirectoryOptions
            {
                Recursive = true
            };

            var allFilesTransferred = new List<CloudBlockBlob>();

            var context = new DirectoryTransferContext();
            context.FileTransferred += (sender, args) => FileTransferredCallback(sender, args, allFilesTransferred);
            context.FileFailed += FileFailedCallback;
            context.FileSkipped += FileSkippedCallback;
            context.ShouldOverwriteCallbackAsync += (source, destination) => Task.FromResult(true);

            await TransferManager.CopyDirectoryAsync(sourceDirectory, destinationDirectory,
                CopyMethod.ServiceSideAsyncCopy, options, context);

            await DeleteBlobsAsync(container, sourceDirectoryPath);
        }

        public async Task UploadFromStreamAsync(string blobName, string contentType, string content)
        {
            await FileStorageUtils.UploadFromStreamAsync(_publicStorageConnectionString, PublicContentContainerName,
                blobName, contentType, content);
        }

        private async Task<List<CloudBlockBlob>> CopyDirectoryAsync(string sourceDirectoryPath, string destinationDirectoryPath,
            CloudBlobContainer sourceContainer, CloudBlobContainer destinationContainer,
            CopyReleaseCommand copyReleaseCommand, ShouldTransferCallbackAsync shouldTransferCallbackAsync)
        {
            var sourceDirectory = sourceContainer.GetDirectoryReference(sourceDirectoryPath);
            var destinationDirectory = destinationContainer.GetDirectoryReference(destinationDirectoryPath);

            var options = new CopyDirectoryOptions
            {
                Recursive = true
            };

            var allFilesTransferred = new List<CloudBlockBlob>();

            var context = new DirectoryTransferContext();
            context.FileTransferred += (sender, args) => FileTransferredCallback(sender, args, allFilesTransferred);
            context.FileFailed += FileFailedCallback;
            context.FileSkipped += FileSkippedCallback;
            context.SetAttributesCallbackAsync += (destination) =>
                SetAttributesCallbackAsync(destination, copyReleaseCommand.PublishScheduled);
            context.ShouldTransferCallbackAsync += shouldTransferCallbackAsync;

            await TransferManager.CopyDirectoryAsync(sourceDirectory, destinationDirectory,
                CopyMethod.ServiceSideAsyncCopy, options, context);

            return allFilesTransferred;
        }

        private async Task ZipFiles(CloudBlobContainer destinationContainer, List<CloudBlockBlob> allFilesTransferred, string destinationDirectoryPath, CopyReleaseCommand copyReleaseCommand)
        {
            var destinationDirectory = destinationContainer.GetDirectoryReference(destinationDirectoryPath);

            await ZipAllFilesToBlob(allFilesTransferred, destinationDirectory, copyReleaseCommand);
        }

        private void FileTransferredCallback(object sender, TransferEventArgs e, List<CloudBlockBlob> allFilesStream)
        {
            var source = (CloudBlockBlob) e.Source;
            var destination = (CloudBlockBlob) e.Destination;
            allFilesStream.Add(source);
            _logger.LogInformation("Transferred {0} -> {1}.", source.Name, destination.Name);
        }

        private void FileFailedCallback(object sender, TransferEventArgs e)
        {
            var source = (CloudBlockBlob) e.Source;
            var destination = (CloudBlockBlob) e.Destination;
            _logger.LogInformation("Failed to transfer {0} -> {1}. Error message:{2}", source.Name, destination.Name,
                e.Exception.Message);
        }

        private void FileSkippedCallback(object sender, TransferEventArgs e)
        {
            var source = (CloudBlockBlob) e.Source;
            var destination = (CloudBlockBlob) e.Destination;
            _logger.LogInformation("Skipped transfer {0} -> {1}.", source.Name, destination.Name);
        }

#pragma warning disable 1998
        private static async Task SetAttributesCallbackAsync(object destination, DateTime releasePublished)
#pragma warning restore 1998
        {
            var releasePublishedString = releasePublished.ToString("o", CultureInfo.InvariantCulture);
            if (destination is CloudBlockBlob cloudBlockBlob)
            {
                cloudBlockBlob.Metadata["releasedatetime"] = releasePublishedString;
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
            
            if (!releaseFileReferences.Exists(rfr => rfr.ReleaseId == releaseId && rfr.Filename == name))
            {
                _logger.LogError($"No release file reference found for releaseId {releaseId} and name : {name}");
                return false;
            }

            return true;
        }

        private List<ReleaseFile> GetAllFilesUploaded(Guid releaseId, params ReleaseFileTypes[] types)
        {
            return _contentDbContext
                .ReleaseFiles
                .Include(f => f.ReleaseFileReference)
                .Where(f => f.ReleaseId == releaseId && types.Contains(f.ReleaseFileReference.ReleaseFileType))
                .ToList();
        }
        
        private static async Task ZipAllFilesToBlob(IEnumerable<CloudBlockBlob> files, CloudBlobDirectory directory,
            CopyReleaseCommand copyReleaseCommand)
        {
            var cloudBlockBlob = CreateBlobForAllFilesZip(directory, copyReleaseCommand);
            var memoryStream = new MemoryStream();

            var zipOutputStream = new ZipOutputStream(memoryStream);
            zipOutputStream.SetLevel(1);

            foreach (var file in files)
            {
                PutNextZipEntry(zipOutputStream, file);
            }

            var context = new SingleTransferContext();
            context.SetAttributesCallbackAsync += (destination) =>
                SetAttributesCallbackAsync(destination, copyReleaseCommand.PublishScheduled);

            zipOutputStream.Finish();
            await TransferManager.UploadAsync(memoryStream, cloudBlockBlob, new UploadOptions(), context);

            zipOutputStream.Close();
        }

        private static void PutNextZipEntry(ZipOutputStream zipOutputStream, CloudBlob cloudBlob)
        {
            var zipEntry = new ZipEntry(GetZipEntryName(cloudBlob));
            zipOutputStream.PutNextEntry(zipEntry);
            cloudBlob.DownloadToStream(zipOutputStream);
        }

        private static string GetZipEntryName(CloudBlob cloudBlob)
        {
            return cloudBlob.Uri.Segments.Last();
        }

        private static string GetZipFilePath(CopyReleaseCommand copyReleaseCommand)
        {
            return $"{Ancillary.GetEnumLabel()}/{copyReleaseCommand.PublicationSlug}_{copyReleaseCommand.ReleaseSlug}.zip";
        }

        private static CloudBlockBlob CreateBlobForAllFilesZip(CloudBlobDirectory directory, CopyReleaseCommand copyReleaseCommand)
        {
            var blob = directory.GetBlockBlobReference(GetZipFilePath(copyReleaseCommand));
            blob.Properties.ContentType = "application/x-zip-compressed";
            blob.Metadata.Add("name", "All files");
            return blob;
        }

        private async Task DeleteBlobsAsync(CloudBlobContainer container, string directoryPath,
            string excludePattern = null)
        {
            var token = new BlobContinuationToken();
            do
            {
                var result = await container.ListBlobsSegmentedAsync(directoryPath,
                    true,
                    BlobListingDetails.None,
                    null,
                    token, null,
                    null);

                token = result.ContinuationToken;

                var items = result.Results
                    .Select(item => item as CloudBlob)
                    .Where(item =>
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
                    });

                await Task.WhenAll(items
                    .Select(item =>
                    {
                        _logger.LogInformation($"Deleting {item.Name}");
                        return item.DeleteIfExistsAsync();
                    })
                );
            } while (token != null);
        }
    }
}
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using GovUk.Education.ExploreEducationStatistics.Common.Extensions;
using GovUk.Education.ExploreEducationStatistics.Common.Functions;
using GovUk.Education.ExploreEducationStatistics.Common.Services;
using GovUk.Education.ExploreEducationStatistics.Publisher.Model;
using GovUk.Education.ExploreEducationStatistics.Publisher.Services.Interfaces;
using ICSharpCode.SharpZipLib.Zip;
using Microsoft.Azure.Storage.Blob;
using Microsoft.Azure.Storage.DataMovement;
using Microsoft.Extensions.Logging;
using static GovUk.Education.ExploreEducationStatistics.Common.Model.ReleaseFileTypes;
using static GovUk.Education.ExploreEducationStatistics.Common.Services.FileStoragePathUtils;
using FileInfo = GovUk.Education.ExploreEducationStatistics.Common.Model.FileInfo;

namespace GovUk.Education.ExploreEducationStatistics.Publisher.Services
{
    public class FileStorageService : IFileStorageService
    {
        private readonly ILogger<FileStorageService> _logger;

        private readonly string _privateStorageConnectionString =
            ConnectionUtils.GetAzureStorageConnectionString("CoreStorage");

        private readonly string _publicStorageConnectionString =
            ConnectionUtils.GetAzureStorageConnectionString("PublicStorage");

        private const string PrivateContainerName = "releases";
        private const string PublicContainerName = "downloads";

        public FileStorageService(ILogger<FileStorageService> logger)
        {
            _logger = logger;
        }

        public async Task CopyReleaseToPublicContainer(PublishReleaseDataMessage message)
        {
            var privateContainer =
                await FileStorageUtils.GetCloudBlobContainerAsync(_privateStorageConnectionString,
                    PrivateContainerName);
            var publicContainer =
                await FileStorageUtils.GetCloudBlobContainerAsync(_publicStorageConnectionString, PublicContainerName);

            var sourceDirectoryPath = AdminReleaseDirectoryPath(message.ReleaseId);
            var destinationDirectoryPath = PublicReleaseDirectoryPath(message.PublicationSlug, message.ReleaseSlug);

            await CopyDirectoryAsyncAndZipFiles(sourceDirectoryPath, destinationDirectoryPath, privateContainer,
                publicContainer, message);
        }

        public IEnumerable<FileInfo> ListPublicFiles(string publication, string release)
        {
            return FileStorageUtils.ListPublicFiles(_publicStorageConnectionString, PublicContainerName, publication,
                release);
        }

        private async Task CopyDirectoryAsyncAndZipFiles(string sourceDirectoryPath, string destinationDirectoryPath,
            CloudBlobContainer sourceContainer, CloudBlobContainer destinationContainer,
            PublishReleaseDataMessage message)
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
                SetAttributesCallbackAsync(destination, message.ReleasePublished);
            context.ShouldTransferCallbackAsync += ShouldTransferCallbackAsync;
            
            await TransferManager.CopyDirectoryAsync(sourceDirectory, destinationDirectory, CopyMethod.ServiceSideAsyncCopy, options, context);
            
            ZipAllFilesToBlob(allFilesTransferred, destinationDirectory, message);
        }

        private void FileTransferredCallback(object sender, TransferEventArgs e, List<CloudBlockBlob> allFilesStream)
        {
            allFilesStream.Add(e.Source as CloudBlockBlob);
            _logger.LogInformation("Transfer succeeds. {0} -> {1}.", e.Source, e.Destination);
        }

        private void FileFailedCallback(object sender, TransferEventArgs e)
        {
            _logger.LogInformation("Transfer fails. {0} -> {1}. Error message:{2}", e.Source, e.Destination,
                e.Exception.Message);
        }

        private void FileSkippedCallback(object sender, TransferEventArgs e)
        {
            _logger.LogInformation("Transfer skips. {0} -> {1}.", e.Source, e.Destination);
        }

#pragma warning disable 1998
        private static async Task SetAttributesCallbackAsync(object destination, DateTime releasePublished)
#pragma warning restore 1998
        {
            var releasePublishedString = releasePublished.ToString("o", CultureInfo.InvariantCulture);
            (destination as CloudBlockBlob)?.Metadata.Add("releasedatetime", releasePublishedString);
        }

        private static async Task<bool> ShouldTransferCallbackAsync(object source, object destination)
        {
            if (source is CloudBlockBlob blob)
            {
                await blob.FetchAttributesAsync();
                return !FileStorageUtils.IsMetaDataFile(blob);
            }

            return true;
        }

        private static async void ZipAllFilesToBlob(IEnumerable<CloudBlockBlob> files, CloudBlobDirectory directory,
            PublishReleaseDataMessage message)
        {
            var cloudBlockBlob = CreateBlobForAllFilesZip(directory, message);
            var memoryStream = new MemoryStream();

            var zipOutputStream = new ZipOutputStream(memoryStream);
            zipOutputStream.SetLevel(1);

            foreach (var file in files)
            {
                PutNextZipEntry(zipOutputStream, file);
            }

            var context = new SingleTransferContext();
            context.SetAttributesCallbackAsync += (destination) =>
                SetAttributesCallbackAsync(destination, message.ReleasePublished);

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

        private static string GetZipFilePath(PublishReleaseDataMessage message)
        {
            return $"{Ancillary.GetEnumLabel()}/{message.PublicationSlug}_{message.ReleaseSlug}.zip";
        }

        private static CloudBlockBlob CreateBlobForAllFilesZip(CloudBlobDirectory directory,
            PublishReleaseDataMessage message)
        {
            var blob = directory.GetBlockBlobReference(GetZipFilePath(message));
            blob.Properties.ContentType = "application/x-zip-compressed";
            blob.Metadata.Add("name", "All files");
            return blob;
        }
    }
}
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using GovUk.Education.ExploreEducationStatistics.Common.Functions;
using GovUk.Education.ExploreEducationStatistics.Common.Services;
using GovUk.Education.ExploreEducationStatistics.Publisher.Model;
using GovUk.Education.ExploreEducationStatistics.Publisher.Services.Interfaces;
using ICSharpCode.SharpZipLib.Zip;
using Microsoft.Azure.Storage.Blob;
using Microsoft.Azure.Storage.DataMovement;
using Microsoft.Extensions.Logging;
using static GovUk.Education.ExploreEducationStatistics.Common.Services.FileStoragePathUtils;

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
                publicContainer, message.ReleasePublished);
        }

        private async Task CopyDirectoryAsyncAndZipFiles(string sourceDirectoryPath, string destinationDirectoryPath,
            CloudBlobContainer sourceContainer, CloudBlobContainer destinationContainer, DateTime releasePublished)
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
            context.ShouldTransferCallbackAsync += ShouldTransferCallbackAsync;

            context.SetAttributesCallbackAsync += async destination =>
            {
                var releasePublishedString = releasePublished.ToString("o", CultureInfo.InvariantCulture);
                (destination as CloudBlockBlob)?.Metadata.Add("releasedatetime", releasePublishedString);
            };

            await TransferManager.CopyDirectoryAsync(sourceDirectory, destinationDirectory, true, options, context);

            ZipAllFilesToBlob(allFilesTransferred, destinationDirectory);
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

        private static async Task<bool> ShouldTransferCallbackAsync(object source, object destination)
        {
            if (source is CloudBlockBlob blob)
            {
                await blob.FetchAttributesAsync();
                return !IsMetaDataFile(blob);
            }

            return true;
        }

        private static bool IsMetaDataFile(CloudBlockBlob blob)
        {
            // The meta data file contains a metadata attribute referencing it's corresponding data file
            return blob.Metadata.ContainsKey(DataFileKey);
        }

        private static async void ZipAllFilesToBlob(IEnumerable<CloudBlockBlob> files, CloudBlobDirectory directory)
        {
            var cloudBlockBlob = CreateBlobForAllFilesZip(directory);
            var memoryStream = new MemoryStream();

            var zipOutputStream = new ZipOutputStream(memoryStream);
            zipOutputStream.SetLevel(1);

            foreach (var file in files)
            {
                PutNextZipEntry(zipOutputStream, file);
            }

            zipOutputStream.Finish();
            await TransferManager.UploadAsync(memoryStream, cloudBlockBlob);

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

        private static CloudBlockBlob CreateBlobForAllFilesZip(CloudBlobDirectory directory)
        {
            var blob = directory.GetBlockBlobReference("all.zip");
            blob.Properties.ContentType = "application/zip";
            return blob;
        }
    }
}
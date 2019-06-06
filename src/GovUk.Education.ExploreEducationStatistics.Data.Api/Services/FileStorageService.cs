using System;
using System.Globalization;
using System.IO;
using System.Threading.Tasks;
using GovUk.Education.ExploreEducationStatistics.Data.Api.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Storage;
using Microsoft.Azure.Storage.Blob;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace GovUk.Education.ExploreEducationStatistics.Data.Api.Services
{
    public class FileStorageService : IFileStorageService
    {
        private readonly string _storageConnectionString;
        private readonly ILogger _logger;

        private const string containerName = "downloads";

        public FileStorageService(IConfiguration config,
            ILogger<FileStorageService> logger)
        {
            _logger = logger;
            _storageConnectionString = config.GetConnectionString("AzureStorage");
        }

        public bool FileExistsAndIsReleased(string publication, string release, string filename)
        {
            var storageAccount = CloudStorageAccount.Parse(_storageConnectionString);

            var blobClient = storageAccount.CreateCloudBlobClient();
            var blobContainer = blobClient.GetContainerReference(containerName);
            var blob = blobContainer.GetBlockBlobReference($"{publication}/{release}/{filename}");

            return blob.Exists() && IsFileReleased(blob);
        }

        public async Task<FileStreamResult> StreamFile(string publication, string release, string filename)
        {
            var storageAccount = CloudStorageAccount.Parse(_storageConnectionString);

            var blobClient = storageAccount.CreateCloudBlobClient();
            var blobContainer = blobClient.GetContainerReference(containerName);
            var blob = blobContainer.GetBlockBlobReference($"{publication}/{release}/{filename}");

            if (!blob.Exists())
            {
                throw new ArgumentException("File not found: {filename}", GetFilePath(blob));
            }

            var stream = new MemoryStream();

            await blob.DownloadToStreamAsync(stream);
            stream.Seek(0, SeekOrigin.Begin);
            return new FileStreamResult(stream, blob.Properties.ContentType)
            {
                FileDownloadName = filename
            };
        }

        private static bool IsFileReleased(CloudBlob blob)
        {
            if (!blob.Exists())
            {
                throw new ArgumentException("File not found: {filename}", GetFilePath(blob));
            }
            
            if (blob.Metadata.TryGetValue("releasedatetime", out var releaseDateTime))
            {
                return DateTime.Compare(ParseDateTime(releaseDateTime), DateTime.Now) <= 0;
            }

            return false;
        }

        private static DateTime ParseDateTime(string dateTime)
        {
            return DateTime.ParseExact(dateTime, "o", CultureInfo.InvariantCulture, DateTimeStyles.None);
        }

        private static string GetFilePath(IListBlobItem blob)
        {
            var path = blob.Uri.LocalPath;
            return path.Substring(path.IndexOf(containerName) + containerName.Length);
        }
    }
}
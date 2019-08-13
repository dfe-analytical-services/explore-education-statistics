using System;
using System.Globalization;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using GovUk.Education.ExploreEducationStatistics.Data.Api.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Storage;
using Microsoft.Azure.Storage.Blob;
using Microsoft.Extensions.Configuration;

namespace GovUk.Education.ExploreEducationStatistics.Data.Api.Services
{
    public class FileStorageService : IFileStorageService
    {
        private readonly string _storageConnectionString;

        public FileStorageService(IConfiguration config)
        {
            _storageConnectionString = config.GetConnectionString("PublicStorage");
        }

        public Task<string> DownloadTextAsync(string containerName, string blobName)
        {
            var storageAccount = CloudStorageAccount.Parse(_storageConnectionString);

            var blobClient = storageAccount.CreateCloudBlobClient();
            var blobContainer = blobClient.GetContainerReference(containerName);
            var blob = blobContainer.GetBlockBlobReference(blobName);

            if (!blob.Exists())
            {
                throw new ArgumentException("File not found: {blobName}", blob.Name);
            }

            return blob.DownloadTextAsync();
        }
        
        public bool FileExistsAndIsReleased(string containerName, string blobName)
        {
            var blobContainer = GetCloudBlobContainer(containerName);
            var blob = blobContainer.GetBlockBlobReference(blobName);
            return blob.Exists() && IsFileReleased(blob);
        }

        public async Task<FileStreamResult> StreamFile(string containerName, string blobName, string fileName)
        {
            var blobContainer = GetCloudBlobContainer(containerName);
            var blob = blobContainer.GetBlockBlobReference(blobName);

            if (!blob.Exists())
            {
                throw new ArgumentException("File not found: {blobName}", blob.Name);
            }

            var stream = new MemoryStream();

            await blob.DownloadToStreamAsync(stream);
            stream.Seek(0, SeekOrigin.Begin);
            return new FileStreamResult(stream, blob.Properties.ContentType)
            {
                FileDownloadName = fileName
            };
        }

        public async Task UploadFromStreamAsync(string containerName, string blobName, string contentType, string content)
        {
            var blobContainer = GetCloudBlobContainer(containerName);
            var blob = blobContainer.GetBlockBlobReference(blobName);
            blob.Properties.ContentType = contentType;
            
            using (Stream stream = new MemoryStream(Encoding.UTF8.GetBytes(content)))
            {
                await blob.UploadFromStreamAsync(stream);
            }
        }

        private static bool IsFileReleased(ICloudBlob blob)
        {
            if (!blob.Exists())
            {
                throw new ArgumentException("File not found: {blobName}", blob.Name);
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
        
        private CloudBlobContainer GetCloudBlobContainer(string containerName)
        {
            var storageAccount = CloudStorageAccount.Parse(_storageConnectionString);
            var blobClient = storageAccount.CreateCloudBlobClient();
            var blobContainer = blobClient.GetContainerReference(containerName);
            blobContainer.CreateIfNotExists();
            return blobContainer;
        }
    }
}
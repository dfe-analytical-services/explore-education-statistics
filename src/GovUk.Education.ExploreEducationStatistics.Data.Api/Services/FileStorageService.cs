using System;
using System.IO;
using System.Threading.Tasks;
using GovUk.Education.ExploreEducationStatistics.Common.Services;
using GovUk.Education.ExploreEducationStatistics.Data.Api.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;

namespace GovUk.Education.ExploreEducationStatistics.Data.Api.Services
{
    public class FileStorageService : IFileStorageService
    {
        private readonly string _storageConnectionString;

        public FileStorageService(IConfiguration config)
        {
            _storageConnectionString = config.GetValue<string>("PublicStorage");
        }

        public async Task<string> DownloadTextAsync(string containerName, string blobName)
        {
            return await FileStorageUtils.DownloadTextAsync(_storageConnectionString, containerName, blobName);
        }

        public bool FileExistsAndIsReleased(string containerName, string blobName)
        {
            var blobContainer = FileStorageUtils.GetCloudBlobContainer(_storageConnectionString, containerName);
            var blob = blobContainer.GetBlockBlobReference(blobName);
            return blob.Exists() && FileStorageUtils.IsFileReleased(blob);
        }

        public async Task<FileStreamResult> StreamFile(string containerName, string blobName, string fileName)
        {
            var blobContainer =
                await FileStorageUtils.GetCloudBlobContainerAsync(_storageConnectionString, containerName);
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

        public async Task UploadFromStreamAsync(string containerName, string blobName, string contentType,
            string content)
        {
            await FileStorageUtils.UploadFromStreamAsync(_storageConnectionString, containerName, blobName,
                contentType, content);
        }
    }
}
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
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

        public FileStorageService(IConfiguration config,
            ILogger<FileStorageService> logger)
        {
            _logger = logger;
            _storageConnectionString = config.GetConnectionString("AzureStorage");
        }

        public bool FileExists(string publication, string release, string filename)
        {
            var storageAccount = CloudStorageAccount.Parse(_storageConnectionString);

            var blobClient = storageAccount.CreateCloudBlobClient();
            var blobContainer = blobClient.GetContainerReference("test");
            var blockBlobReference = blobContainer.GetBlockBlobReference($"{publication}/{release}/{filename}");

            return blockBlobReference.Exists();
        }

        public async Task<FileStreamResult> StreamFile(string publication, string release, string filename)
        {
            var storageAccount = CloudStorageAccount.Parse(_storageConnectionString);

            var blobClient = storageAccount.CreateCloudBlobClient();
            var blobContainer = blobClient.GetContainerReference("test");
            var blockBlobReference = blobContainer.GetBlockBlobReference($"{publication}/{release}/{filename}");

            if (!blockBlobReference.Exists())
            {
                throw new ArgumentException("File not found: {filename}", filename);
            }

            var stream = new MemoryStream();

            await blockBlobReference.DownloadToStreamAsync(stream);
            stream.Seek(0, SeekOrigin.Begin);
            return new FileStreamResult(stream, "application/octet-stream")
            {
                FileDownloadName = filename
            };
        }

        public IEnumerable<string> ListFiles(string publication, string release)
        {
            var storageAccount = CloudStorageAccount.Parse(_storageConnectionString);

            var blobClient = storageAccount.CreateCloudBlobClient();
            var blobContainer = blobClient.GetContainerReference("test");

            var list = blobContainer.ListBlobs($"{publication}/{release}", true);

            return list.Select(blob => blob.StorageUri.PrimaryUri.LocalPath);
        }
    }
}
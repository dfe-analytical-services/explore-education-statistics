using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using GovUk.Education.ExploreEducationStatistics.Common.Services;
using GovUk.Education.ExploreEducationStatistics.Data.Api.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Storage.Blob;
using Microsoft.Extensions.Configuration;
using static GovUk.Education.ExploreEducationStatistics.Common.Services.FileStorageUtils;

namespace GovUk.Education.ExploreEducationStatistics.Data.Api.Services
{
    public class FileStorageService : IFileStorageService
    {
        private readonly string _storageConnectionString;

        public FileStorageService(string connectionString)
        {
            _storageConnectionString = connectionString;
        }
        

        public async Task<string> DownloadTextAsync(string containerName, string blobName)
        {
            return await FileStorageUtils.DownloadTextAsync(_storageConnectionString, containerName, blobName);
        }

        public IEnumerable<CloudBlockBlob> ListBlobs(string containerName)
        {
            return FileStorageUtils.ListBlobs(_storageConnectionString, containerName);
        }

        public CloudBlob GetBlob(string containerName, string blobName)
        {
            return FileStorageUtils.GetBlob(_storageConnectionString, containerName, blobName);
        }

        public bool FileExistsAndIsReleased(string containerName, string blobName)
        {
            var blob = GetBlob(containerName, blobName);
            return blob.Exists() && IsFileReleased(blob);
        }

        public async Task<FileStreamResult> StreamFile(string containerName, string blobName, string fileName)
        {
            var blob = GetBlockBlob(_storageConnectionString, containerName, blobName);

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

        public Task AppendFromStreamAsync(string containerName, string blobName, string contentType,
            string content)
        {
            return FileStorageUtils.AppendFromStreamAsync(_storageConnectionString, containerName, blobName,
                contentType, content);
        }
        
        public Task UploadFromStreamAsync(string containerName, string blobName, string contentType,
            string content)
        {
            return FileStorageUtils.UploadFromStreamAsync(_storageConnectionString, containerName, blobName,
                contentType, content);
        }
        
        public Task<bool> TryGetOrCreateAppendBlobAsync(string containerName, string blobName)
        {
            return FileStorageUtils.TryGetOrCreateAppendBlobAsync(_storageConnectionString, containerName, blobName);
        }
    }
}

using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using GovUk.Education.ExploreEducationStatistics.Common.Extensions;
using GovUk.Education.ExploreEducationStatistics.Common.Model;
using GovUk.Education.ExploreEducationStatistics.Common.Services.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Data.Api.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace GovUk.Education.ExploreEducationStatistics.Data.Api.Services
{
    public class FileStorageService : IFileStorageService
    {
        private readonly IBlobStorageService _blobStorageService;

        public FileStorageService(IBlobStorageService blobStorageService)
        {
            _blobStorageService = blobStorageService;
        }


        public async Task<string> GetBlobText(string containerName, string path)
        {
            return await _blobStorageService.DownloadBlobText(containerName, path);
        }

        public async Task<IEnumerable<BlobInfo>> ListBlobs(string containerName)
        {
            return await _blobStorageService.ListBlobs(containerName);
        }

        public async Task<bool> CheckBlobExists(string containerName, string path)
        {
            return await _blobStorageService.CheckBlobExists(containerName, path);
        }

        public async Task<BlobInfo> GetBlob(string containerName, string path)
        {
            return await _blobStorageService.GetBlob(containerName, path);
        }

        public async Task<bool> IsBlobReleased(string containerName, string path)
        {
            var blob = await _blobStorageService.GetBlob(containerName, path);

            return blob.IsReleased();
        }

        public async Task<FileStreamResult> StreamFile(string containerName, string path)
        {
            var blob = await _blobStorageService.GetBlob(containerName, path);

            var stream = new MemoryStream();
            await _blobStorageService.DownloadToStream(containerName, path, stream);

            return new FileStreamResult(stream, blob.ContentType)
            {
                FileDownloadName = blob.FileName,
            };
        }

        public Task AppendText(
            string containerName,
            string path,
            string contentType,
            string content)
        {
            return _blobStorageService.AppendText(
                containerName: containerName,
                path: path,
                content: content
            );
        }

        public Task UploadText(string containerName, string path, string contentType, string content)
        {
            return _blobStorageService.UploadText(
                containerName: containerName,
                path: path,
                content: content,
                contentType: contentType
            );
        }

        public Task<bool> IsAppendSupported(string containerName, string path)
        {
            return _blobStorageService.IsAppendSupported(containerName, path);
        }
    }
}
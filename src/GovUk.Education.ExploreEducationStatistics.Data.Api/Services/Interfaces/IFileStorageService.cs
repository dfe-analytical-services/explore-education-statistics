using System.Collections.Generic;
using System.Threading.Tasks;
using GovUk.Education.ExploreEducationStatistics.Common.Model;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Storage.Blob;

namespace GovUk.Education.ExploreEducationStatistics.Data.Api.Services.Interfaces
{
    public interface IFileStorageService
    {
        Task AppendText(string containerName, string path, string contentType, string content);

        Task<string> GetBlobText(string containerName, string path);

        Task<bool> IsBlobReleased(string containerName, string path);

        Task<bool> CheckBlobExists(string containerName, string path);

        Task<BlobInfo> GetBlob(string containerName, string path);

        Task<IEnumerable<BlobInfo>> ListBlobs(string containerName);

        Task<bool> IsAppendSupported(string containerName, string path);

        Task<FileStreamResult> StreamFile(string containerName, string path);

        Task UploadText(string containerName, string path, string contentType, string content);
    }
}
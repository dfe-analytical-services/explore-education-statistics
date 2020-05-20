using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Storage.Blob;

namespace GovUk.Education.ExploreEducationStatistics.Data.Api.Services.Interfaces
{
    public interface IFileStorageService
    {
        Task AppendFromStreamAsync(string containerName, string blobName, string contentType, string content);
        
        Task<string> DownloadTextAsync(string containerName, string blobName);

        bool FileExistsAndIsReleased(string containerName, string blobName);

        CloudBlob GetBlob(string containerName, string blobName);
        
        IEnumerable<CloudBlockBlob> ListBlobs(string containerName);

        Task<bool> TryGetOrCreateAppendBlobAsync(string containerName, string blobName);

        Task<FileStreamResult> StreamFile(string containerName, string blobName, string fileName);

        Task UploadFromStreamAsync(string containerName, string blobName, string contentType, string content);
    }
}
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;

namespace GovUk.Education.ExploreEducationStatistics.Data.Api.Services.Interfaces
{
    public interface IFileStorageService
    {
        Task<string> DownloadTextAsync(string containerName, string blobName);

        bool FileExistsAndIsReleased(string containerName, string blobName);

        Task<FileStreamResult> StreamFile(string containerName, string blobName, string fileName);

        Task UploadFromStreamAsync(string containerName, string blobName, string contentType, string content);
    }
}
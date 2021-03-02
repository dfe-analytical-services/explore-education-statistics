using System.Threading.Tasks;
using GovUk.Education.ExploreEducationStatistics.Common;
using GovUk.Education.ExploreEducationStatistics.Common.Model;
using GovUk.Education.ExploreEducationStatistics.Content.Model;
using GovUk.Education.ExploreEducationStatistics.Publisher.Models;
using Newtonsoft.Json;

namespace GovUk.Education.ExploreEducationStatistics.Publisher.Services.Interfaces
{
    public interface IFileStorageService
    {
        Task<BlobLease> AcquireLease(string blobName);

        Task<bool> CheckBlobExists(IBlobContainer containerName, string path);

        Task CopyReleaseFilesToPublicContainer(CopyReleaseFilesCommand copyReleaseFilesCommand);

        Task DeleteAllContentAsyncExcludingStaging();

        Task DeleteDownloadFilesForPreviousVersion(Release release);

        Task DeletePublicBlobs(string directoryPath, string excludePattern = null);

        Task DeletePublicBlob(string path);

        Task<BlobInfo> GetBlob(IBlobContainer containerName, string path);

        Task MovePublicDirectory(IBlobContainer containerName, string sourceDirectoryPath, string destinationDirectoryPath);

        Task UploadAsJson(string filePath, object value, JsonSerializerSettings settings = null);
    }
}

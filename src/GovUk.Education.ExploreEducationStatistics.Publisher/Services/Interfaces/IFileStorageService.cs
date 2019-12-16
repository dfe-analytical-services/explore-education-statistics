using System.Collections.Generic;
using System.Threading.Tasks;
using GovUk.Education.ExploreEducationStatistics.Common.Model;
using GovUk.Education.ExploreEducationStatistics.Publisher.Model;

namespace GovUk.Education.ExploreEducationStatistics.Publisher.Services.Interfaces
{
    public interface IFileStorageService
    {
        Task CopyReleaseToPublicContainer(PublishReleaseFilesMessage message);

        IEnumerable<FileInfo> ListPublicFiles(string publication, string release);

        Task UploadFromStreamAsync(string storageConnectionString, string containerName, string blobName,
            string contentType, string content);
    }
}